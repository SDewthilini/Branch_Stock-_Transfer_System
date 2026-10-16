namespace StockTransferApi.Models
{
    // Lists the possible stages of a stock transfer.
    public enum TransferStatus
    {
        Requested = 1,
        Dispatched = 2,
        Received = 3,
        Cancelled = 4
    }
}
