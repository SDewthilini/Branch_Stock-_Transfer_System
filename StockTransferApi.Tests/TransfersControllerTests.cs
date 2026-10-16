using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTransferApi.Controllers;
using StockTransferApi.Data;
using StockTransferApi.DTOs;
using StockTransferApi.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace StockTransferApi.Tests
{
    public class TransfersControllerTests
    {
        // Helper method to create an isolated in-memory database context for each unit test
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // Transactions Ignore කිරීමට
                .Options;

            return new AppDbContext(options);
        }
        // =========================================================================
        // TEST 1: Stock can never go below zero (Over-stock dispatch prevention)
        // =========================================================================
        [Fact]
        public async Task DispatchTransfer_InsufficientStock_ReturnsBadRequestAndPreventsNegativeStock()
        {
            // Arrange: Set available stock to 5, but request 10 units
            using var context = GetInMemoryDbContext();

            context.Stocks.Add(new Stock { Id = 1, BranchId = 1, ProductId = 1, Quantity = 5 });

            var transfer = new Transfer
            {
                Id = 1,
                FromBranchId = 1,
                ToBranchId = 2,
                Status = TransferStatus.Requested,
                TransferLines = new List<TransferLine>
                {
                    new TransferLine { ProductId = 1, Quantity = 10 }
                }
            };
            context.Transfers.Add(transfer);
            await context.SaveChangesAsync();

            var controller = new TransfersController(context);

            // Act: Attempt to dispatch the over-stock transfer
            var result = await controller.DispatchTransfer(1);

            // Assert: Ensure it returns BadRequest and stock remains unchanged
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Insufficient stock", badRequestResult.Value?.ToString() ?? "");

            var stock = await context.Stocks.FirstOrDefaultAsync(s => s.BranchId == 1 && s.ProductId == 1);
            Assert.NotNull(stock);
            Assert.Equal(5, stock.Quantity);
        }

        // =========================================================================
        // TEST 2: Prevent double dispatch (Cannot dispatch an already dispatched transfer)
        // =========================================================================
        [Fact]
        public async Task DispatchTransfer_AlreadyDispatched_ReturnsBadRequest()
        {
            // Arrange: Transfer is already in 'Dispatched' status
            using var context = GetInMemoryDbContext();

            var transfer = new Transfer
            {
                Id = 1,
                FromBranchId = 1,
                ToBranchId = 2,
                Status = TransferStatus.Dispatched,
                TransferLines = new List<TransferLine>
                {
                    new TransferLine { ProductId = 1, Quantity = 2 }
                }
            };
            context.Transfers.Add(transfer);
            await context.SaveChangesAsync();

            var controller = new TransfersController(context);

            // Act: Attempt to dispatch for the second time
            var result = await controller.DispatchTransfer(1);

            // Assert: Ensure rejection via BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Cannot dispatch", badRequestResult.Value?.ToString() ?? "");
        }

        // =========================================================================
        // TEST 3: Valid receive operation (Updates status and increments destination stock)
        // =========================================================================
        [Fact]
        public async Task ReceiveTransfer_ValidDispatchedTransfer_IncrementsTargetBranchStock()
        {
            // Arrange: Transfer is in 'Dispatched' status with 8 units
            using var context = GetInMemoryDbContext();

            var transfer = new Transfer
            {
                Id = 1,
                FromBranchId = 1,
                ToBranchId = 2, // Target Branch
                Status = TransferStatus.Dispatched,
                TransferLines = new List<TransferLine>
                {
                    new TransferLine { ProductId = 1, Quantity = 8 }
                }
            };
            context.Transfers.Add(transfer);
            await context.SaveChangesAsync();

            var controller = new TransfersController(context);

            // Act: Receive the transfer
            var result = await controller.ReceiveTransfer(1);

            // Assert: Verify status is 'Received' and target branch stock increased by 8
            Assert.IsType<OkObjectResult>(result);

            var updatedTransfer = await context.Transfers.FindAsync(1);
            Assert.NotNull(updatedTransfer);
            Assert.Equal(TransferStatus.Received, updatedTransfer.Status);

            var targetStock = await context.Stocks
                .FirstOrDefaultAsync(s => s.BranchId == 2 && s.ProductId == 1);

            Assert.NotNull(targetStock);
            Assert.Equal(8, targetStock.Quantity);
        }

        // =========================================================================
        // TEST 4: Status restriction rule (Cannot receive a transfer that was not dispatched)
        // =========================================================================
        [Fact]
        public async Task ReceiveTransfer_NotDispatchedYet_ReturnsBadRequest()
        {
            // Arrange: Status is still 'Requested'
            using var context = GetInMemoryDbContext();

            var transfer = new Transfer
            {
                Id = 1,
                FromBranchId = 1,
                ToBranchId = 2,
                Status = TransferStatus.Requested,
                TransferLines = new List<TransferLine>
                {
                    new TransferLine { ProductId = 1, Quantity = 5 }
                }
            };
            context.Transfers.Add(transfer);
            await context.SaveChangesAsync();

            var controller = new TransfersController(context);

            // Act: Attempt to receive before dispatching
            var result = await controller.ReceiveTransfer(1);

            // Assert: Ensure it returns BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Cannot receive", badRequestResult.Value?.ToString() ?? "");
        }

        // =========================================================================
        // TEST 5: From and To branches must be different validation rule
        // =========================================================================
        [Fact]
        public async Task CreateTransfer_SameFromAndToBranch_ReturnsBadRequest()
        {
            // Arrange: Source and Destination branches are identical (ID = 1)
            using var context = GetInMemoryDbContext();
            var controller = new TransfersController(context);

            var dto = new CreateTransferDto
            {
                FromBranchId = 1,
                ToBranchId = 1,
                Items = new List<CreateTransferLineDto>
                {
                    new CreateTransferLineDto { ProductId = 1, Quantity = 5 }
                }
            };

            // Act: Attempt to create transfer
            var result = await controller.CreateTransfer(dto);

            // Assert: Ensure validation catches it and returns BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("From Branch and To Branch cannot be the same.", badRequestResult.Value);
        }

        // =========================================================================
        // TEST 6: Quantity must be greater than zero validation rule
        // =========================================================================
        [Fact]
        public async Task CreateTransfer_ZeroOrNegativeQuantity_ReturnsBadRequest()
        {
            // Arrange: Transfer quantity is set to 0
            using var context = GetInMemoryDbContext();
            var controller = new TransfersController(context);

            var dto = new CreateTransferDto
            {
                FromBranchId = 1,
                ToBranchId = 2,
                Items = new List<CreateTransferLineDto>
                {
                    new CreateTransferLineDto { ProductId = 1, Quantity = 0 }
                }
            };

            // Act: Attempt to create transfer
            var result = await controller.CreateTransfer(dto);

            // Assert: Ensure validation rejects zero quantity
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Quantity for all items must be greater than 0.", badRequestResult.Value);
        }
    }
}