using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class Ledger
    {
        public int Id { get; set; }
        public int CertificateId { get; set; }
        public string EventType { get; set; }
        public int ElectricityProductionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Volume { get; set; }

        // Navigation properties
        public Certificate Certificate { get; set; }
        public ElectricityProduction ElectricityProduction { get; set; }
    }
}
