namespace StockTransferApi.DTOs
{
    // Carries the branch IDs and requested items for a new transfer.
    public class CreateTransferDto
    {
        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }
        public List<CreateTransferLineDto> Items { get; set; } = new();
    }

    // Describes one product and quantity in a transfer request.
    public class CreateTransferLineDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
