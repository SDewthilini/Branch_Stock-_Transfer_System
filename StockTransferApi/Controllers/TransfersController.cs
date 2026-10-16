using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTransferApi.Data;
using StockTransferApi.DTOs;
using StockTransferApi.Models;

namespace StockTransferApi.Controllers
{
    // Handles transfer requests and their dispatch, receipt, and cancellation.
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransfersController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/transfers?branchId=2&status=Requested
        [HttpGet]
        public async Task<IActionResult> GetTransfers([FromQuery] int? branchId, [FromQuery] TransferStatus? status)
        {
            var query = _context.Transfers
                .Include(t => t.FromBranch)
                .Include(t => t.ToBranch)
                .Include(t => t.TransferLines)
                .ThenInclude(tl => tl.Product)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(t => t.FromBranchId == branchId.Value || t.ToBranchId == branchId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var transfers = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Ok(transfers);
        }

        // POST /api/transfers
        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto dto)
        {
            // Rule 4: From Branch and To Branch cannot be the same
            if (dto.FromBranchId == dto.ToBranchId)
                return BadRequest("From Branch and To Branch cannot be the same.");

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Transfer must contain at least one item.");

            // Rule 4: Quantity must be greater than 0
            if (dto.Items.Any(i => i.Quantity <= 0))
                return BadRequest("Quantity for all items must be greater than 0.");

            var transfer = new Transfer
            {
                FromBranchId = dto.FromBranchId,
                ToBranchId = dto.ToBranchId,
                Status = TransferStatus.Requested,
                CreatedAt = DateTime.UtcNow,
                TransferLines = dto.Items.Select(i => new TransferLine
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            return Ok(transfer);
        }

        // POST /api/transfers/{id}/dispatch
        [HttpPost("{id}/dispatch")]
        public async Task<IActionResult> DispatchTransfer(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.Transfers
                    .Include(t => t.TransferLines)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transfer == null)
                    return NotFound("Transfer request not found.");

                // Rule 2: Cannot dispatch twice
                if (transfer.Status != TransferStatus.Requested)
                    return BadRequest($"Cannot dispatch. Current transfer status is '{transfer.Status}'. Only 'Requested' transfers can be dispatched.");

                // Deduct Stock from Source Branch
                foreach (var line in transfer.TransferLines)
                {
                    var sourceStock = await _context.Stocks
                        .FirstOrDefaultAsync(s => s.BranchId == transfer.FromBranchId && s.ProductId == line.ProductId);

                    // Rule 1: Stock quantity cannot be negative / Over-stock dispatch check
                    if (sourceStock == null || sourceStock.Quantity < line.Quantity)
                    {
                        return BadRequest($"Insufficient stock for Product ID {line.ProductId} at source branch. Available: {sourceStock?.Quantity ?? 0}, Requested: {line.Quantity}");
                    }

                    sourceStock.Quantity -= line.Quantity;
                }

                transfer.Status = TransferStatus.Dispatched;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(transfer);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST /api/transfers/{id}/receive
        [HttpPost("{id}/receive")]
        public async Task<IActionResult> ReceiveTransfer(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.Transfers
                    .Include(t => t.TransferLines)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transfer == null)
                    return NotFound("Transfer request not found.");

                // Rule 3: Cannot receive unless dispatched
                if (transfer.Status != TransferStatus.Dispatched)
                    return BadRequest($"Cannot receive. Current transfer status is '{transfer.Status}'. Only 'Dispatched' transfers can be received.");

                // Add Stock to Target Branch
                foreach (var line in transfer.TransferLines)
                {
                    var targetStock = await _context.Stocks
                        .FirstOrDefaultAsync(s => s.BranchId == transfer.ToBranchId && s.ProductId == line.ProductId);

                    if (targetStock == null)
                    {
                        // If stock record doesn't exist for destination branch, create it
                        targetStock = new Stock
                        {
                            BranchId = transfer.ToBranchId,
                            ProductId = line.ProductId,
                            Quantity = line.Quantity
                        };
                        _context.Stocks.Add(targetStock);
                    }
                    else
                    {
                        targetStock.Quantity += line.Quantity;
                    }
                }

                transfer.Status = TransferStatus.Received;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(transfer);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST /api/transfers/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelTransfer(int id)
        {
            var transfer = await _context.Transfers.FindAsync(id);

            if (transfer == null)
                return NotFound("Transfer request not found.");

            // Can only cancel if in 'Requested' status
            if (transfer.Status != TransferStatus.Requested)
                return BadRequest($"Cannot cancel transfer. Status is '{transfer.Status}'.");

            transfer.Status = TransferStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok(transfer);
        }
    }
}
