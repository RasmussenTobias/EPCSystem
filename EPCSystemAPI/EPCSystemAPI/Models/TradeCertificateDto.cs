using System.ComponentModel.DataAnnotations;

namespace EPCSystemAPI.models
{
    public class TradeCertificateDto
    {
        public int id {  get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public List<CertificateTransferDto> Transfers { get; set; }
    }
}