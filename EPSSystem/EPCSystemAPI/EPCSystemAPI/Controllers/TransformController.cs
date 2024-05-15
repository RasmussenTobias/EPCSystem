using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EPCSystemAPI.Models;  // Ensure correct namespace
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransformController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransformController> _logger;

        public TransformController(ApplicationDbContext context, ILogger<TransformController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("transform")]
        public async Task<IActionResult> TransformCertificate([FromBody] TransformRequestDto transformRequest)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validate that the user has enough certificates for the inputs
                    foreach (var input in transformRequest.Inputs)
                    {
                        var certificate = await _context.Certificates
                            .Include(c => c.ElectricityProduction)
                            .FirstOrDefaultAsync(c => c.Id == input.CertificateId && c.UserId == transformRequest.UserId);

                        if (certificate == null)
                        {
                            return NotFound($"Certificate with ID {input.CertificateId} not found for user {transformRequest.UserId}");
                        }

                        if (certificate.CurrentVolume < input.Amount)
                        {
                            return BadRequest($"Insufficient volume for certificate ID {input.CertificateId}. Available: {certificate.CurrentVolume}, Required: {input.Amount}");
                        }

                        // Update the current volume of the input certificate
                        certificate.CurrentVolume -= input.Amount;
                        _context.Certificates.Update(certificate);

                        // Get the deviceId from the ElectricityProduction table
                        var electricityProduction = await _context.Electricity_Production
                            .FirstOrDefaultAsync(ep => ep.Id == certificate.ElectricityProductionId);

                        if (electricityProduction == null)
                        {
                            return NotFound($"ElectricityProduction with ID {certificate.ElectricityProductionId} not found");
                        }

                        var deviceId = electricityProduction.DeviceId;

                        // Log transformation event for input
                        var inputTransformEvent = new TransformEvent
                        {
                            OriginalCertificateId = certificate.Id,
                            DeviceId = deviceId,
                            TransformationDetails = transformRequest.TransformationDetails,
                            TransformationTimestamp = DateTime.UtcNow,
                            TransformedVolume = -input.Amount,
                            PreviousTransformEventId = null // Update if needed
                        };
                        await _context.TransformEvents.AddAsync(inputTransformEvent);
                    }

                    // Create output certificates and log transformation events
                    foreach (var output in transformRequest.Outputs)
                    {
                        // Create new certificate for the output
                        var newCertificate = new Certificate
                        {
                            UserId = transformRequest.UserId,
                            ElectricityProductionId = output.ElectricityProductionId,
                            CreatedAt = DateTime.UtcNow,
                            Volume = output.Amount,
                            CurrentVolume = output.Amount
                        };
                        await _context.Certificates.AddAsync(newCertificate);
                        await _context.SaveChangesAsync();

                        // Log transformation event for output
                        var outputTransformEvent = new TransformEvent
                        {
                            OriginalCertificateId = newCertificate.Id,
                            DeviceId = output.DeviceId,
                            TransformationDetails = transformRequest.TransformationDetails,
                            TransformationTimestamp = DateTime.UtcNow,
                            TransformedVolume = output.Amount,
                            PreviousTransformEventId = null // Update if needed
                        };
                        await _context.TransformEvents.AddAsync(outputTransformEvent);
                    }

                    await transaction.CommitAsync();
                    return Ok("Transformation successful");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during transformation: {Message}", ex.Message);
                    return StatusCode(500, "Internal server error during transformation.");
                }
            }
        }
    }
}
