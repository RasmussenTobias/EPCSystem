using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransferEvent
    {
        [Key]
        public int Id { get; set; }
        public int BundleId { get; set; }
        public int Electricity_Production_Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public decimal Volume { get; set; }       

        [ForeignKey("FromUserId")]
        public User FromUser { get; set; }

        [ForeignKey("ToUserId")]
        public User ToUser { get; set; }

    }
}
