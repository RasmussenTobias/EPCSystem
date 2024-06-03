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
        public async Task<IActionResult> BacktrackCertificates([FromQuery] int certificateId)
        {
            try
            {
                var result = await GetCertificateHistory(certificateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backtracking: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error during backtracking: {ex.Message}");
            }
        }

        [HttpGet("originals")]
        public async Task<IActionResult> GetOriginalCertificates([FromQuery] int certificateId)
        {
            try
            {
                var backtrackResult = await GetCertificateHistory(certificateId);
                var originalCertificates = ExtractOriginalCertificates(backtrackResult);
                return Ok(originalCertificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during fetching original certificates: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error during fetching original certificates: {ex.Message}");
            }
        }

        private async Task<CertificateHistory> GetCertificateHistory(int certificateId)
        {
            var processedCertificates = new HashSet<(int, int)>(); // Using a tuple to track certificate and bundle id
            return await BuildCertificateHistory(certificateId, processedCertificates);
        }

        private List<CertificateHistory> ExtractOriginalCertificates(CertificateHistory certificateHistory)
        {
            var originalCertificates = new Dictionary<int, CertificateHistory>();
            TraverseHistory(certificateHistory, originalCertificates);
            return originalCertificates.Values.ToList();
        }

        private void TraverseHistory(CertificateHistory history, Dictionary<int, CertificateHistory> originalCertificates)
        {
            if (history.Inputs.Count == 0)
            {
                if (originalCertificates.ContainsKey(history.CertificateId))
                {
                    originalCertificates[history.CertificateId].TransformedVolume += history.TransformedVolume ?? 0;
                }
                else
                {
                    originalCertificates[history.CertificateId] = new CertificateHistory
                    {
                        CertificateId = history.CertificateId,
                        DeviceId = history.DeviceId,
                        PowerType = history.PowerType,
                        DeviceName = history.DeviceName,
                        DeviceType = history.DeviceType,
                        DeviceLocation = history.DeviceLocation,
                        TransformedVolume = history.TransformedVolume ?? 0,
                        TransformationTimestamp = history.TransformationTimestamp
                    };
                }
            }
            else
            {
                foreach (var input in history.Inputs)
                {
                    TraverseHistory(input, originalCertificates);
                }
            }
        }

        private async Task<CertificateHistory> BuildCertificateHistory(int certificateId, HashSet<(int, int)> processedCertificates)
        {
            var certificate = await _context.Certificates
                .Include(c => c.ElectricityProduction)
                .ThenInclude(ep => ep.Device)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            if (certificate == null)
            {
                _logger.LogError($"Certificate with ID {certificateId} not found.");
                return new CertificateHistory
                {
                    CertificateId = certificateId,
                    Error = "Certificate not found"
                };
            }

            var history = new CertificateHistory
            {
                CertificateId = certificate.Id,
                DeviceId = certificate.ElectricityProduction?.DeviceId,
                PowerType = certificate.ElectricityProduction?.Device?.PowerType,
                DeviceName = certificate.ElectricityProduction?.Device?.DeviceName,
                DeviceType = certificate.ElectricityProduction?.Device?.DeviceType,
                DeviceLocation = certificate.ElectricityProduction?.Device?.Location,
                TransformationTimestamp = certificate.CreatedAt // Assuming the certificate's creation time as the transformation time
            };

            processedCertificates.Add((certificateId, certificate.ElectricityProduction?.DeviceId ?? 0));

            // Find all transform events where this certificate was created as an output
            var transformEventsAsNew = await _context.TransformEvents
                .Where(te => te.NewCertificateId == certificateId)
                .OrderBy(te => te.TransformationTimestamp)
                .ToListAsync();

            foreach (var transformEvent in transformEventsAsNew)
            {
                // Get all events with the same BundleId
                var inputs = await _context.TransformEvents
                    .Where(te => te.BundleId == transformEvent.BundleId)
                    .ToListAsync();

                foreach (var inputEvent in inputs)
                {
                    var key = (inputEvent.RootCertificateId, inputEvent.BundleId.Value);
                    if (!processedCertificates.Contains(key))
                    {
                        processedCertificates.Add(key);
                        var inputHistory = await BuildCertificateHistory(inputEvent.RootCertificateId, processedCertificates);
                        if (inputHistory != null)
                        {
                            inputHistory.TransformedVolume = inputEvent.TransformedVolume;
                            history.Inputs.Add(inputHistory);
                        }
                    }
                }
            }

            return history;
        }
    }

    public class CertificateHistory
    {
        public int CertificateId { get; set; }
        public int? DeviceId { get; set; }
        public string PowerType { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceLocation { get; set; }
        public decimal? TransformedVolume { get; set; }
        public DateTime TransformationTimestamp { get; set; }
        public string Error { get; set; }
        public List<CertificateHistory> Inputs { get; set; } = new List<CertificateHistory>();
    }
}
