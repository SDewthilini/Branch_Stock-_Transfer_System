namespace StockTransferApi.Models
{
    // Represents one product and quantity included in a transfer.
    public class TransferLine
    {
        public int Id { get; set; }
        public int TransferId { get; set; }
        public Transfer Transfer { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
