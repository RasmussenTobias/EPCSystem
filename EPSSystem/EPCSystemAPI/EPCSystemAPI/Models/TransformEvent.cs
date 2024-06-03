using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformEvent
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BundleId { get; set; }
        public decimal TransformedVolume { get; set; }
        public DateTime TransformationTimestamp { get; set; }
        public int RootCertificateId { get; set; } // Field to track the original certificate
        public int NewCertificateId { get; set; }

        public Certificate RootCertificate { get; set; }
    }

}
