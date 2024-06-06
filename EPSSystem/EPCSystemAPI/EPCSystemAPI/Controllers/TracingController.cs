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
    public class TracingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TracingController> _logger;

        public TracingController(ApplicationDbContext context, ILogger<TracingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("trace")]
        public async Task<IActionResult> TraceCertificates([FromQuery] int certificateId)
        {
            try
            {
                var history = await GetCertificateHistory(certificateId);
                var totalEmissions = CalculateTotalEmissions(history);

                var response = new CertificateHistoryResponse
                {
                    TotalEmissions = totalEmissions,
                    Tracing = history
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tracing: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error during tracing: {ex.Message}");
            }
        }

        private async Task<CertificateHistory> GetCertificateHistory(int certificateId)
        {
            var processedCertificates = new HashSet<(int, int)>(); // Using a tuple to track certificate and bundle id
            return await BuildCertificateHistory(certificateId, processedCertificates);
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

            var emissionFactor = certificate.ElectricityProduction?.Device?.EmissionFactor ?? 0;

            var history = new CertificateHistory
            {
                CertificateId = certificate.Id,
                DeviceId = certificate.ElectricityProduction?.DeviceId,
                PowerType = certificate.ElectricityProduction?.Device?.PowerType,
                DeviceName = certificate.ElectricityProduction?.Device?.DeviceName,
                DeviceType = certificate.ElectricityProduction?.Device?.DeviceType,
                DeviceLocation = certificate.ElectricityProduction?.Device?.Location,
                EmissionFactor = emissionFactor,
                TransformedVolume = certificate.CurrentVolume,
                TransformationTimestamp = certificate.CreatedAt
            };

            processedCertificates.Add((certificateId, certificate.ElectricityProduction?.DeviceId ?? 0));

            // Find all transform events where this certificate was created as an output
            var transformEventsAsNew = await _context.TransformEvents
                .Where(te => te.NewCertificateId == certificateId)
                .OrderBy(te => te.TransformationTimestamp)
                .ToListAsync();

            if (transformEventsAsNew.Any())
            {
                decimal totalInputVolume = 0;
                foreach (var transformEvent in transformEventsAsNew)
                {
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

                                // If the input has no further inputs, its input volume is its transformed volume
                                if (inputHistory.Inputs.Count == 0)
                                {
                                    inputHistory.InputVolume = inputHistory.TransformedVolume ?? 0;
                                }
                                else
                                {
                                    // Sum the input volumes of all inputs
                                    inputHistory.InputVolume = inputHistory.Inputs.Sum(i => i.TransformedVolume ?? 0);
                                }

                                inputHistory.TotalEmissions = (inputHistory.InputVolume ?? 0) * inputHistory.EmissionFactor.GetValueOrDefault();
                                history.Inputs.Add(inputHistory);
                                totalInputVolume += inputEvent.TransformedVolume; // Use the TransformedVolume from TransformEvents
                            }
                        }
                    }
                }
                history.InputVolume = totalInputVolume;
            }
            else
            {
                // If no inputs, the input volume should be the same as the transformed volume
                history.InputVolume = history.TransformedVolume ?? 0;
            }

            history.TotalEmissions = (history.InputVolume ?? 0) * emissionFactor;
            return history;
        }




        private decimal CalculateTotalEmissions(CertificateHistory certificateHistory)
        {
            var totalEmissions = certificateHistory.TotalEmissions ?? 0;

            foreach (var input in certificateHistory.Inputs)
            {
                totalEmissions += CalculateTotalEmissions(input);
            }

            return totalEmissions;
        }
    }

    public class CertificateHistoryResponse
    {
        public decimal TotalEmissions { get; set; }
        public CertificateHistory Tracing { get; set; }
    }

    public class CertificateHistory
    {
        public int CertificateId { get; set; }
        public int? DeviceId { get; set; }
        public string PowerType { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceLocation { get; set; }
        public decimal? EmissionFactor { get; set; }
        public decimal? TransformedVolume { get; set; }
        public decimal? InputVolume { get; set; }
        public decimal? TotalEmissions { get; set; }
        public DateTime TransformationTimestamp { get; set; }
        public string Error { get; set; }
        public List<CertificateHistory> Inputs { get; set; } = new List<CertificateHistory>();
    }
}
