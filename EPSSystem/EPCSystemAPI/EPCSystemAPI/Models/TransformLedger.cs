namespace EPCSystemAPI.models
{
    public class TransformLedger
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int ElectricityProductionId { get; set; }
        public string EventType { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal AmountWh { get; set; }
    }

}
