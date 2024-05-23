using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformEvent
    {
        public int Id { get; set; }
        public int OriginalCertificateId { get; set; }
        public int NewCertificateId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int BundleId { get; set; }
        public decimal TransformedVolume { get; set; }
        public string TransformationDetails { get; set; }
        public DateTime TransformationTimestamp { get; set; }

        public Certificate OriginalCertificate { get; set; }
        public Certificate NewCertificate { get; set; }
        public User FromUser { get; set; }
        public User ToUser { get; set; }
    }
}
