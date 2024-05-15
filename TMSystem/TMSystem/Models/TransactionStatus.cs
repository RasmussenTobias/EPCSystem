namespace TMSystem.Models
{
    public class TransactionStatus
    {
        public Guid TransactionId { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }


}
