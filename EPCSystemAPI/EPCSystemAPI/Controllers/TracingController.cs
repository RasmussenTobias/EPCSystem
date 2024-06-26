﻿using Microsoft.AspNetCore.Mvc;
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
                    .Include(c => c.EnergyProduction)
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
            var emissionFactor = certificate.EnergyProduction?.Device?.EmissionFactor ?? 0;
            var history = new CertificateHistory
            {
                CertificateId = certificate.Id,
                DeviceId = certificate.EnergyProduction?.DeviceId ?? -1, // Handle nullable DeviceId
                PowerType = certificate.EnergyProduction?.Device?.PowerType,
                DeviceName = certificate.EnergyProduction?.Device?.DeviceName,
                DeviceType = certificate.EnergyProduction?.Device?.DeviceType,
                DeviceLocation = certificate.EnergyProduction?.Device?.Location,
                EmissionFactor = emissionFactor,
                TransformedVolume = certificate.CurrentVolume,
                TransformationTimestamp = certificate.CreatedAt
            };

            processedCertificates.Add((certificate.Id, certificate.EnergyProduction?.DeviceId ?? -1));

            var transformEventsAsNew = await _context.TransformEvents
                .Where(te => te.NewCertificateId == certificate.Id && te.UserId != 0)
                .OrderBy(te => te.TransformationTimestamp)
                .ToListAsync();

            decimal totalInputVolume = 0;
            foreach (var transformEvent in transformEventsAsNew)
            {
                var inputs = await _context.TransformEvents
                    .Where(te => te.BundleId == transformEvent.BundleId && te.UserId != 0)
                    .ToListAsync();

                foreach (var inputEvent in inputs)
                {
                    var key = (inputEvent.RootCertificateId ?? -1, inputEvent.BundleId.Value);
                    if (!processedCertificates.Contains(key))
                    {
                        processedCertificates.Add(key);
                        var inputCertificate = await _context.Certificates
                            .Include(c => c.EnergyProduction)
                            .ThenInclude(ep => ep.Device)
                            .FirstOrDefaultAsync(c => c.Id == inputEvent.RootCertificateId);

                        if (inputCertificate != null)
                        {
                            var inputHistory = await BuildCertificateHistory(inputCertificate, processedCertificates);
                            if (inputEvent.UserId != 0)
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
            }
            history.InputVolume = totalInputVolume;
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
