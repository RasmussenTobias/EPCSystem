using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformEvent
    {
        [Key]
        public int Id { get; set; }
        public int LedgerId { get; set; }
        public int DeviceId { get; set; }
        public int ElectricityProductionId { get; set; }
        public decimal AmountWh { get; set; }

        [ForeignKey("LedgerId")]
        public Ledger Ledger { get; set; }

        [ForeignKey("DeviceId")]
        public Device Device { get; set; }

        [ForeignKey("ElectricityProductionId")]
        public ElectricityProduction ElectricityProduction { get; set; }
    }
}
