namespace StockTransferApi.Models
{
    // Represents a warehouse or branch location.
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsWarehouse { get; set; } = false;

        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}
