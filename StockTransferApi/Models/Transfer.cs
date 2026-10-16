namespace StockTransferApi.Models
{
    // Represents a stock transfer between two branches.
    public class Transfer
    {
        public int Id { get; set; }
        public int FromBranchId { get; set; }
        public Branch FromBranch { get; set; } = null!;

        public int ToBranchId { get; set; }
        public Branch ToBranch { get; set; } = null!;

        public TransferStatus Status { get; set; } = TransferStatus.Requested;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TransferLine> TransferLines { get; set; } = new List<TransferLine>();
    }
}
