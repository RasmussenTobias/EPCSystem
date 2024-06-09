using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class ElectricityProduction
    {
        public int Id { get; set; }
        public DateTime ProductionTime { get; set; }
        public decimal AmountWh { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
    }
}
