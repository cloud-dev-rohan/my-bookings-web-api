namespace MyBookingsWebApi.Models
{
    public class Inventory

    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RemainingCount { get; set; }
        private DateTime _expirationDate;

        public DateTime ExpirationDate
        {
            get => _expirationDate;
            set => _expirationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
