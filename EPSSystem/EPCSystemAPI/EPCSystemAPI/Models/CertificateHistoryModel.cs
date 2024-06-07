using System;
using System.Collections.Generic;

namespace EPCSystemAPI.models
{
    public class CertificateHistoryResponse
    {
        public decimal TotalEmissions { get; set; }
        public CertificateHistory Tracing { get; set; }
    }

    public class CertificateHistory
    {
        public int CertificateId { get; set; }
        public int? DeviceId { get; set; }
        public string PowerType { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceLocation { get; set; }
        public decimal? EmissionFactor { get; set; }
        public decimal? TransformedVolume { get; set; }
        public decimal? InputVolume { get; set; }
        public decimal? TotalEmissions { get; set; }
        public DateTime TransformationTimestamp { get; set; }
        public string Error { get; set; }
        public List<CertificateHistory> Inputs { get; set; } = new List<CertificateHistory>();
    }
}
