namespace StockTransferApi.DTOs
{
    // Represents product stock returned for a branch.
    public class StockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
