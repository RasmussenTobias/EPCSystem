using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class UserEventView
    {
        public int EventId { get; set; }
        public string EventType { get; set; }
        public int ReferenceId { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
        public int? EnergyProductionId { get; set; }
        public int? CertificateId { get; set; }
    }

}