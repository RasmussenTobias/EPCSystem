using System;
using System.Collections.Generic;  // Add this to use List
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformRequestDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string TransformationDetails { get; set; }
        public List<CertificateInputDto> Inputs { get; set; }
        public decimal Loss { get; set; } // Property for transformation loss
    }

    public class CertificateInputDto
    {
        public int CertificateId { get; set; }
        public decimal Amount { get; set; }
    }    
}
