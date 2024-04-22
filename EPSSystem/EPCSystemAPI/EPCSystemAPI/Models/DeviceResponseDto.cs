using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{   
    public class DeviceResponseDto
    {
        public int Id { get; set; }
        public string DeviceName { get; set; }
        public string Location { get; set; }
        public decimal TotalProduction { get; set; } // Add TotalProduction property
    }
}