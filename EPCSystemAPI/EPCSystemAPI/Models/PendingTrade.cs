using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class PendingTrade
    {
        public int Id { get; set; }
        public int BundleId { get; set; } // Added BulkId
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public decimal Volume { get; set; }
        public int ElectricityProductionId { get; set; }         
    }
}
