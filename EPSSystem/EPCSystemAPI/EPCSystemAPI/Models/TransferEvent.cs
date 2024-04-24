using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransferEvent
    {
        [Key]
        public int Id { get; set; }
        public int LedgerId { get; set; }
        //public int CertificateId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public decimal Volume { get; set; }

        [ForeignKey("LedgerId")]
        public Ledger Ledger { get; set; }

        [ForeignKey("FromUserId")]
        public User FromUser { get; set; }

        [ForeignKey("ToUserId")]
        public User ToUser { get; set; }

    }
}
