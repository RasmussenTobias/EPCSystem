using System;
using System.Collections.Generic;  // Add this to use List
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransformRequestDto
    {
        public int DeviceId { get; set; }
        public DateTime ProductionTime { get; set; }
        public decimal AmountWh { get; set; }
        public List<TransformInputDto> Inputs { get; set; }

        public decimal Efficiency { get; set; }

    }

    public class TransformInputDto
    {
        public int CertificateId { get; set; }
        public decimal Amount { get; set; }
    }

}
