namespace EPCSystemAPI.models
{
    public class ProduceLedger
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public int EnergyProductionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? Volume { get; set; }
    }

}
