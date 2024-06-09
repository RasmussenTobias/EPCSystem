using System;
using System.Collections.Generic;

namespace EPCSystemAPI.models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public ICollection <Device> Devices { get; set; } = new List<Device>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    }

}
