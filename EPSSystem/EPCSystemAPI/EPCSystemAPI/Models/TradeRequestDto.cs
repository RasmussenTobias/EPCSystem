using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TradeRequestDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public List<OfferedCertificateDto> OfferedCertificates { get; set; }
        public List<RequestedCertificateDto> RequestedCertificates { get; set; }
    }

    public class OfferedCertificateDto
    {
        public int Amount { get; set; }
        public int ElectricityProductionId { get; set; } // Added property for currency
    }

    public class RequestedCertificateDto
    {
        public int Amount { get; set; }
        public int ElectricityProductionId { get; set; } // Added property for currency
    }
}
