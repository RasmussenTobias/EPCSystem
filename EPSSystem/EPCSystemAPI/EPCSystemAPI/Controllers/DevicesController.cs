using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPCSystemAPI.models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevicesController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
        {
            return await _context.Devices.ToListAsync();
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> GetDevicesByUsername(string username)
        {
            // Find the devices associated with the user and select specific properties
            var devices = await _context.Devices
                .Include(d => d.User)
                .Include(d => d.ElectricityProductions) // Include related ElectricityProductions
                .Where(d => d.User.Username == username)
                .Select(d => new DeviceResponseDto
                {
                    Id = d.Id,
                    DeviceName = d.DeviceName,
                    Location = d.Location,
                    TotalProduction = d.ElectricityProductions.Sum(ep => ep.AmountWh) // Calculate total production
                })
                .ToListAsync();

            if (devices == null || devices.Count == 0)
            {
                return NotFound("No devices found for the specified username");
            }

            return devices;
        }







        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(DeviceDto deviceInput)
        {
            var device = new Device
            {
                UserId = deviceInput.UserId,
                DeviceName = deviceInput.DeviceName,
                Location = deviceInput.Location,
                CreatedAt = DateTime.UtcNow 
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevices), new { id = device.Id }, device);
        }
    }
}