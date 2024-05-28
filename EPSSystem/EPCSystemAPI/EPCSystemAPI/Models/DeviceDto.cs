using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class DeviceDto
    {
        public int UserId { get; set; }
        public string DeviceName { get; set; }
        public string PowerType { get; set; }
        public string DeviceType { get; set; }
        public string Location { get; set; }
    }
}