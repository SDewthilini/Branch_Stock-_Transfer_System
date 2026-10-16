using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTransferApi.Data;
using StockTransferApi.DTOs;

namespace StockTransferApi.Controllers
{
    // Provides endpoints for branch details and branch stock.
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BranchesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // Returns all branches.
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _context.Branches.ToListAsync();
            return Ok(branches);
        }

        [HttpGet("{id}/stock")]
        // Returns the products and quantities held by a branch.
        public async Task<IActionResult> GetBranchStock(int id)
        {
            var stockList = await _context.Stocks
                .Where(s => s.BranchId == id)
                .Include(s => s.Product)
                .Select(s => new StockDto
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity
                })
                .ToListAsync();

            return Ok(stockList);
        }
    }
}
