using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPCSystemAPI.Models;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BacktrackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BacktrackController> _logger;

        public BacktrackController(ApplicationDbContext context, ILogger<BacktrackController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("backtrack")]
        public async Task<IActionResult> BacktrackCertificates([FromQuery] List<int> certificateIds)
        {
            var results = new List<object>();

            try
            {
                foreach (var certificateId in certificateIds)
                {
                    var certificate = await _context.Certificates
                        .Include(c => c.ElectricityProduction)
                        .ThenInclude(ep => ep.Device)
                        .FirstOrDefaultAsync(c => c.Id == certificateId);

                    if (certificate == null)
                    {
                        _logger.LogError($"Certificate with ID {certificateId} not found.");
                        results.Add(new { CertificateId = certificateId, Error = "Certificate not found" });
                        continue;
                    }

                    var originDevice = await FindOriginDevice(certificateId);

                    if (originDevice == null)
                    {
                        _logger.LogError($"Origin device for certificate ID {certificateId} not found.");
                        results.Add(new { CertificateId = certificateId, Error = "Origin device not found" });
                        continue;
                    }

                    results.Add(new
                    {
                        CertificateId = certificateId,
                        DeviceId = originDevice.Id,
                        PowerType = originDevice.PowerType,
                        DeviceName = originDevice.DeviceName,
                        DeviceType = originDevice.DeviceType,
                        DeviceLocation = originDevice.Location
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backtracking: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error during backtracking: {ex.Message}");
            }
        }

        private async Task<Device> FindOriginDevice(int certificateId)
        {
            var currentCertificateId = certificateId;
            while (true)
            {
                var transformEvent = await _context.TransformEvents
                    .Where(te => te.NewCertificateId == currentCertificateId)
                    .OrderByDescending(te => te.TransformationTimestamp)
                    .FirstOrDefaultAsync();

                if (transformEvent == null)
                {
                    var certificate = await _context.Certificates
                        .Include(c => c.ElectricityProduction)
                        .ThenInclude(ep => ep.Device)
                        .FirstOrDefaultAsync(c => c.Id == currentCertificateId);

                    return certificate?.ElectricityProduction?.Device;
                }

                currentCertificateId = transformEvent.OriginalCertificateId;
            }
        }
    }
}
