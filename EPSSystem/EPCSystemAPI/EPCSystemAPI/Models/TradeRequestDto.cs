using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public enum TradeResponseType
    {
        Commit,
        Abort
    }
    public class TradeRequestDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public List<CertificateTransferDto> OfferedCertificates { get; set; }
        public TradeResponseType TradeResponseType { get; set; }
    }
}
