using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class Device
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DeviceName { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
