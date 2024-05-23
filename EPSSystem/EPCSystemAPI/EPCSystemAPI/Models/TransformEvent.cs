using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformEvent
    {
        public int Id { get; set; }
        public int? OriginalCertificateId { get; set; }
        public string TransformationDetails { get; set; }
        public DateTime TransformationTimestamp { get; set; }
        public decimal TransformedVolume { get; set; }
        public int? PreviousTransformEventId { get; set; }

        public Certificate OriginalCertificate { get; set; }
        public TransformEvent PreviousTransformEvent { get; set; }
    }
}
