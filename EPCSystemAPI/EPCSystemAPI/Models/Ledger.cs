using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class Ledger
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Certificate")]
        public int CertificateId { get; set; }

        [Required]
        public string EventType { get; set; }

        [ForeignKey("ElectricityProduction")]
        public int ElectricityProductionId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public decimal Volume { get; set; }

        // Navigation properties
        public Certificate Certificate { get; set; }
        public ElectricityProduction ElectricityProduction { get; set; }
    }
}
