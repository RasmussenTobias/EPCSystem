using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformRequestDto
    {
        public int UserId { get; set; }
        public string TransformationDetails { get; set; }
        public List<CertificateInputDto> Inputs { get; set; }
        public List<CertificateOutputDto> Outputs { get; set; }
    }

    public class CertificateInputDto
    {
        public int CertificateId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CertificateOutputDto
    {
        public int ElectricityProductionId { get; set; }
        public decimal Amount { get; set; }
        public int DeviceId { get; set; } // Ensure you have this field to log the event properly
    }
}
