using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTransferApi.Data;
using StockTransferApi.Models;

namespace StockTransferApi.Controllers
{
    // Provides endpoints for product information.
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        // Returns all products.
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
    }
}
