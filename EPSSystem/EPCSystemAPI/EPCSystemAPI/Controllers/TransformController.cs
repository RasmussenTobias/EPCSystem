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
                    int bundleId = await _context.TransformEvents.MaxAsync(te => (int?)te.BundleId) ?? 0;
                    bundleId++;

                    foreach (var input in transformRequest.Inputs)
                    {
                        var fromCertificate = await _context.Certificates
                            .Include(c => c.ElectricityProduction)
                            .FirstOrDefaultAsync(c => c.Id == input.CertificateId && c.UserId == transformRequest.FromUserId);

                        if (fromCertificate == null)
                        {
                            _logger.LogError($"Certificate with ID {input.CertificateId} not found for user {transformRequest.FromUserId}");
                            return NotFound($"Certificate with ID {input.CertificateId} not found for user {transformRequest.FromUserId}");
                        }

                        if (fromCertificate.CurrentVolume < input.Amount)
                        {
                            _logger.LogError($"Insufficient volume for certificate ID {input.CertificateId}. Available: {fromCertificate.CurrentVolume}, Required: {input.Amount}");
                            return BadRequest($"Insufficient volume for certificate ID {input.CertificateId}. Available: {fromCertificate.CurrentVolume}, Required: {input.Amount}");
                        }

                        // Update the current volume of the fromCertificate
                        fromCertificate.CurrentVolume -= input.Amount;
                        _context.Entry(fromCertificate).State = EntityState.Modified; // Ensure the certificate is marked as modified

                        // Create a new certificate for the ToUser with the input amount
                        var newCertificate = new Certificate
                        {
                            UserId = transformRequest.ToUserId,
                            ElectricityProductionId = fromCertificate.ElectricityProductionId,
                            CreatedAt = DateTime.UtcNow,
                            Volume = input.Amount,
                            CurrentVolume = input.Amount
                        };
                        await _context.Certificates.AddAsync(newCertificate);
                        await _context.SaveChangesAsync();

                        // Log transformation event
                        var transformEvent = new TransformEvent
                        {
                            OriginalCertificateId = fromCertificate.Id,
                            NewCertificateId = newCertificate.Id,
                            FromUserId = transformRequest.FromUserId,
                            ToUserId = transformRequest.ToUserId,
                            BundleId = bundleId,
                            TransformedVolume = input.Amount,
                            TransformationDetails = transformRequest.TransformationDetails,
                            TransformationTimestamp = DateTime.UtcNow
                        };
                        await _context.TransformEvents.AddAsync(transformEvent);
                        await _context.SaveChangesAsync();

                        // Create an event record for the transformation
                        var eventRecord = new Event
                        {
                            Event_Type = "TRANSFORM",
                            Reference_Id = transformEvent.Id,
                            User_Id = transformRequest.FromUserId,
                            Timestamp = DateTime.UtcNow
                        };
                        await _context.Events.AddAsync(eventRecord);
                        await _context.SaveChangesAsync();
                    }

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
