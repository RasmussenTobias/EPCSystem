using System;

namespace EPCSystemAPI.models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }

        public ICollection<Device> Devices { get; set; } // Navigation property to Device
        public ICollection<Certificate> Certificates { get; set; } // Navigation property to Certificate

    }

}
