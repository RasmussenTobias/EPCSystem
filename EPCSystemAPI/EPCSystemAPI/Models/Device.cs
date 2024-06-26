﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class Device
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string DeviceName { get; set; }
        public string PowerType { get; set; }
        public string DeviceType { get; set; }
        public decimal EmissionFactor { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public ICollection<EnergyProduction> EnergyProductions { get; set; } = new List<EnergyProduction>();
    }
}
