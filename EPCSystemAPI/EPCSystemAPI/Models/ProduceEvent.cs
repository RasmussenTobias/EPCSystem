using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class ProduceEvent
    {
        public int Id { get; set; }

        public int Event_Id { get; set; }
        public int DeviceId { get; set; }
        public int EnergyProductionId { get; set; }
        public DateTime ProductionTime { get; set; }

        // Navigation properties
        public Device Device { get; set; }
        public EnergyProduction EnergyProduction { get; set; }
    }
}
