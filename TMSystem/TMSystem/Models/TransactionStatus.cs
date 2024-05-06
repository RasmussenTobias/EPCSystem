namespace TMSystem.Models
{
    public class TransactionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? TransactionId { get; set; } // Optional, based on your logging or tracking requirements
    }


}
