using System.ComponentModel.DataAnnotations;

namespace EPCSystemAPI.models
{
    public class CertificateTransferDto
    {
        public int CertificateId { get; set; }
        public decimal Amount { get; set; } 
    }
}
