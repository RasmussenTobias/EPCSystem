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
                    TransformEvent lastTransformEvent = null;

                    // Validate that the user has enough certificates for the inputs
                    foreach (var input in transformRequest.Inputs)
                    {
                        var certificate = await _context.Certificates
                            .Include(c => c.ElectricityProduction)
                            .FirstOrDefaultAsync(c => c.Id == input.CertificateId && c.UserId == transformRequest.FromUserId);

                        if (certificate == null)
                        {
                            _logger.LogError($"Certificate with ID {input.CertificateId} not found for user {transformRequest.FromUserId}");
                            return NotFound($"Certificate with ID {input.CertificateId} not found for user {transformRequest.FromUserId}");
                        }

                        if (certificate.CurrentVolume < input.Amount)
                        {
                            _logger.LogError($"Insufficient volume for certificate ID {input.CertificateId}. Available: {certificate.CurrentVolume}, Required: {input.Amount}");
                            return BadRequest($"Insufficient volume for certificate ID {input.CertificateId}. Available: {certificate.CurrentVolume}, Required: {input.Amount}");
                        }

                        // Update the current volume of the input certificate
                        certificate.CurrentVolume -= input.Amount;
                        _context.Entry(certificate).State = EntityState.Modified; // Ensure the certificate is marked as modified

                        // Log transformation event for input
                        _logger.LogInformation("Creating transformation event for input certificate ID {CertificateId}", certificate.Id);
                        var inputTransformEvent = new TransformEvent
                        {
                            OriginalCertificateId = certificate.Id,
                            TransformationDetails = transformRequest.TransformationDetails,
                            TransformationTimestamp = DateTime.UtcNow,
                            TransformedVolume = -input.Amount,
                            PreviousTransformEventId = lastTransformEvent?.Id
                        };
                        await _context.TransformEvents.AddAsync(inputTransformEvent);
                        await _context.SaveChangesAsync();

                        // Set lastTransformEvent to the current inputTransformEvent
                        lastTransformEvent = inputTransformEvent;
                    }

                    // Calculate total input volume and total loss
                    var totalInputVolume = transformRequest.Inputs.Sum(input => input.Amount);
                    var totalLoss = transformRequest.Loss;

                    // Calculate the adjusted output volume after accounting for loss
                    var adjustedOutputVolume = totalInputVolume - totalLoss;

                    if (adjustedOutputVolume < 0)
                    {
                        adjustedOutputVolume = 0; // Ensure we do not have negative amounts
                    }

                    // Create a new certificate for the ToUser with the adjusted output volume
                    var firstInputCertificate = await _context.Certificates
                        .FirstOrDefaultAsync(c => c.Id == transformRequest.Inputs.First().CertificateId);

                    if (firstInputCertificate == null)
                    {
                        _logger.LogError("First input certificate not found for the transformation.");
                        return NotFound("First input certificate not found for the transformation.");
                    }

                    var newCertificate = new Certificate
                    {
                        UserId = transformRequest.ToUserId,
                        ElectricityProductionId = firstInputCertificate.ElectricityProductionId,
                        CreatedAt = DateTime.UtcNow,
                        Volume = adjustedOutputVolume,
                        CurrentVolume = adjustedOutputVolume
                    };
                    await _context.Certificates.AddAsync(newCertificate);
                    await _context.SaveChangesAsync();

                    // Log transformation event for the output
                    _logger.LogInformation("Creating transformation event for new certificate ID {CertificateId}", newCertificate.Id);
                    var outputTransformEvent = new TransformEvent
                    {
                        OriginalCertificateId = newCertificate.Id,
                        TransformationDetails = transformRequest.TransformationDetails,
                        TransformationTimestamp = DateTime.UtcNow,
                        TransformedVolume = adjustedOutputVolume,
                        PreviousTransformEventId = lastTransformEvent?.Id // Link to the last input transform event
                    };
                    await _context.TransformEvents.AddAsync(outputTransformEvent);

                    // Commit the transaction
                    await transaction.CommitAsync();
                    return Ok("Transformation and transfer successful");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var baseException = ex.GetBaseException();
                    _logger.LogError(baseException, "Error during transformation: {Message}", baseException.Message);
                    return StatusCode(500, $"Internal server error during transformation: {baseException.Message}");
                }
            }
        }
    }
}
