using System;

namespace EPCSystemAPI.models
{
    public class EnergyProductionDto
    {
        public DateTime ProductionTime { get; set; }
        public decimal AmountWh { get; set; }
        public int DeviceId { get; set; }
    }
}
