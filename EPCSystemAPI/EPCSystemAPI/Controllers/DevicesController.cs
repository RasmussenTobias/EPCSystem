using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPCSystemAPI.models;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        // Dependency injection for dbcontext
        private readonly ApplicationDbContext _context;

        // Constructor with dependency injection
        public DevicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
        {
            return await _context.Devices.ToListAsync();
        }

        // Get devices by username
        [HttpGet("{username}")]
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> GetDevicesByUsername(string username)
        {
            // Query database for devices by username, include related data
            var devices = await _context.Devices
                .Include(d => d.User)
                .Include(d => d.EnergyProductions)
                .Where(d => d.User.Username == username)
                .Select(d => new DeviceResponseDto
                {
                    Id = d.Id,
                    DeviceName = d.DeviceName,
                    Location = d.Location,
                    TotalProduction = d.EnergyProductions.Sum(ep => ep.AmountWh)
                })
                .ToListAsync();

            // Error message if no devices are found
            if (devices == null || devices.Count == 0)
            {
                return NotFound("No devices found for the specified username");
            }

            return devices;
        }

        // Post a new device
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(DeviceDto deviceInput)
        {
            // Check if a device with the same name exists for the same user
            bool deviceExists = await _context.Devices.AnyAsync(d =>
                d.UserId == deviceInput.UserId && d.DeviceName == deviceInput.DeviceName);

            // Return a conflict response if the device already exists
            if (deviceExists)
            {
                return Conflict("A device with the same name already exists for this user.");
            }

            // Create and save new device if no conflict exists
            var device = new Device
            {
                UserId = deviceInput.UserId,
                DeviceName = deviceInput.DeviceName,
                PowerType = deviceInput.PowerType,
                DeviceType = deviceInput.DeviceType,
                Location = deviceInput.Location,
                EmissionFactor = deviceInput.EmissionFactor,
                CreatedAt = DateTime.UtcNow
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            // Respond with the created device
            return CreatedAtAction(nameof(GetDevices), new { id = device.Id }, device);
        }
    }
}
