namespace TMSystem.Models
{
    public class TransactionRequest
    {
        public Guid TransactionId { get; set; } // Unique identifier for the transaction
        public int FromAccountId { get; set; } // EPS or BS account ID
        public int ToAccountId { get; set; } // EPS or BS account ID
        public decimal Amount { get; set; } // Amount to be transferred
        public string ResourceType { get; set; } // Could be money or certificates
    }

}
