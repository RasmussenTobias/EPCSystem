namespace TMSystem.Models
{
    public class TransactionLog
    {
        public Guid TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } // Could be "initiate", "pre-commit", "commit", "rollback"
        public string Details { get; set; } // Additional details about the transaction step
    }
}