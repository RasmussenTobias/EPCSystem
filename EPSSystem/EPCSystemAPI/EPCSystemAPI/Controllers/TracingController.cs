using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TracingController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TracingController> _logger;

        // Dependency constructor
        public TracingController(ApplicationDbContext context, ILogger<TracingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET endpoint to trace the history of a certificate by the certificates ID
        [HttpGet("trace")]
        public async Task<IActionResult> TraceCertificates([FromQuery] int certificateId)
        {
            try
            {
                // Check if the certificate exists
                var certificate = await _context.Certificates
                    .Include(c => c.ElectricityProduction)
                    .ThenInclude(ep => ep.Device)
                    .FirstOrDefaultAsync(c => c.Id == certificateId);

                if (certificate == null)
                {
                    _logger.LogWarning($"Certificate with ID {certificateId} not found.");
                    return NotFound(new { error = $"Certificate with ID {certificateId} not found." });
                }

                // Creates the model for the output
                var history = await GetCertificateHistory(certificate);
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

        // Retrieves the history of a specific certificate
        private async Task<CertificateHistory> GetCertificateHistory(Certificate certificate)
        {
            var processedCertificates = new HashSet<(int, int)>();
            return await BuildCertificateHistory(certificate, processedCertificates);
        }

        // Recursive method to build a history trace for the given certificate
        private async Task<CertificateHistory> BuildCertificateHistory(Certificate certificate, HashSet<(int, int)> processedCertificates)
        {
            // Prepares the history object
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

            processedCertificates.Add((certificate.Id, certificate.ElectricityProduction?.DeviceId ?? 0));

            // Explore further the history through transform events linked to this certificate
            var transformEventsAsNew = await _context.TransformEvents
                .Where(te => te.NewCertificateId == certificate.Id)
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
                            var inputCertificate = await _context.Certificates
                                .Include(c => c.ElectricityProduction)
                                .ThenInclude(ep => ep.Device)
                                .FirstOrDefaultAsync(c => c.Id == inputEvent.RootCertificateId);

                            var inputHistory = await BuildCertificateHistory(inputCertificate, processedCertificates);
                            if (inputHistory != null)
                            {
                                inputHistory.TransformedVolume = inputEvent.TransformedVolume;
                                if (inputHistory.Inputs.Count == 0)
                                {
                                    inputHistory.InputVolume = inputHistory.TransformedVolume ?? 0;
                                }
                                else
                                {
                                    inputHistory.InputVolume = inputHistory.Inputs.Sum(i => i.TransformedVolume ?? 0);
                                }

                                inputHistory.TotalEmissions = (inputHistory.InputVolume ?? 0) * inputHistory.EmissionFactor.GetValueOrDefault();
                                history.Inputs.Add(inputHistory);
                                totalInputVolume += inputEvent.TransformedVolume;
                            }
                        }
                    }
                }
                history.InputVolume = totalInputVolume;
            }
            else
            {
                history.InputVolume = history.TransformedVolume ?? 0;
            }

            history.TotalEmissions = (history.InputVolume ?? 0) * emissionFactor;
            return history;
        }

        // Calculates total emissions recursively for the given certificate and its inputs
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
}
