namespace MyBookingsWebApi.Models
{
    public class InventoryCsvDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RemainingCount { get; set; }
        public DateTime ExpirationDate { get; set; }
        
    }
}
