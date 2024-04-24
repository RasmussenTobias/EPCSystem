using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class Certificate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ElectricityProductionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal volume { get; set; }

        // Navigation properties
        public User User { get; set; }
        public ElectricityProduction ElectricityProduction { get; set; }
    }
}
