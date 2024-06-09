namespace EPCSystemAPI.models
{
    public class TransferLedger
    {
        public int Id { get; set; }
        //public int CertificateId { get; set; }
        public int userId { get; set; }
        public string EventType { get; set; }
        public int EnergyProductionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? Volume { get; set; }
        public int TransferEventId { get; set; }
    }
}
