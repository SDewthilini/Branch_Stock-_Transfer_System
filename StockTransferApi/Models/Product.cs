namespace StockTransferApi.Models
{
    // Represents a product that can be stored and transferred.
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
    }
}
