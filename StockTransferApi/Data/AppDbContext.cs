using Microsoft.EntityFrameworkCore;
using StockTransferApi.Models;

namespace StockTransferApi.Data
{
    // Connects the API models to database tables and configures their relationships.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<TransferLine> TransferLines => Set<TransferLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Stock Table - Unique constraint for BranchId + ProductId
            modelBuilder.Entity<Stock>()
                .HasIndex(s => new { s.BranchId, s.ProductId })
                .IsUnique();

            // Transfer Relationship Configurations
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.FromBranch)
                .WithMany()
                .HasForeignKey(t => t.FromBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.ToBranch)
                .WithMany()
                .HasForeignKey(t => t.ToBranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- SEED DATA ---
            modelBuilder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "Main Warehouse", IsWarehouse = true },
                new Branch { Id = 2, Name = "Colombo Branch", IsWarehouse = false },
                new Branch { Id = 3, Name = "Kandy Branch", IsWarehouse = false },
                new Branch { Id = 4, Name = "Galle Branch", IsWarehouse = false }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Rice (5kg)", SKU = "RICE-5KG" },
                new Product { Id = 2, Name = "Sugar (1kg)", SKU = "SUGAR-1KG" },
                new Product { Id = 3, Name = "Milk Powder (400g)", SKU = "MILK-400G" },
                new Product { Id = 4, Name = "Biscuits Pack", SKU = "DISC-PACK" },
                new Product { Id = 5, Name = "Soap Bar", SKU = "SOAP-BAR" }
            );

            // Initial Stock Setup
            modelBuilder.Entity<Stock>().HasData(
                // Main Warehouse Initial Stock
                new Stock { Id = 1, BranchId = 1, ProductId = 1, Quantity = 500 },
                new Stock { Id = 2, BranchId = 1, ProductId = 2, Quantity = 500 },
                new Stock { Id = 3, BranchId = 1, ProductId = 3, Quantity = 500 },
                new Stock { Id = 4, BranchId = 1, ProductId = 4, Quantity = 500 },
                new Stock { Id = 5, BranchId = 1, ProductId = 5, Quantity = 500 },

                // Colombo Initial Stock
                new Stock { Id = 6, BranchId = 2, ProductId = 1, Quantity = 100 },
                new Stock { Id = 7, BranchId = 2, ProductId = 2, Quantity = 80 },

                // Kandy Initial Stock
                new Stock { Id = 8, BranchId = 3, ProductId = 1, Quantity = 20 },
                new Stock { Id = 9, BranchId = 3, ProductId = 2, Quantity = 15 }
            );
        }
    }
}
