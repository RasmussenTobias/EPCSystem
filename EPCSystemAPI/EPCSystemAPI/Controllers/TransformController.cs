using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.Services;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransformController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransformController> _logger;
        private readonly EnergyProductionService _productionService;

        // Dependency constructor
        public TransformController(ApplicationDbContext context, ILogger<TransformController> logger, EnergyProductionService productionService)
        {
            _context = context;
            _logger = logger;
            _productionService = productionService;
        }

        [HttpPost("transform")]
        // POST endpoint to handle energy certificates transformation
        public async Task<IActionResult> TransformCertificate([FromBody] TransformRequestDto transformRequest)
        {            
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the device using DeviceId, validate existence
                    var device = await _context.Devices.FindAsync(transformRequest.DeviceId);
                    if (device == null)
                    {
                        _logger.LogError($"Device with ID {transformRequest.DeviceId} not found.");
                        return NotFound($"Device with ID {transformRequest.DeviceId} not found.");
                    }

                    int deviceOwnerId = device.UserId;
                    decimal totalInputVolume = 0;
                    // Determine the next bundle ID
                    var bundleId = await _context.TransformEvents.MaxAsync(te => (int?)te.BundleId) ?? 0;
                    bundleId++;

                    // Process each certificate input
                    foreach (var input in transformRequest.Inputs)
                    {
                        // Check each certificate for validity and sufficient volume
                        var certificate = await _context.Certificates.FirstOrDefaultAsync(c => c.Id == input.CertificateId && c.UserId == deviceOwnerId);
                        if (certificate == null || certificate.CurrentVolume < input.Amount)
                        {
                            _logger.LogError($"Insufficient volume or certificate not found for certificate ID {input.CertificateId}.");
                            return BadRequest($"Insufficient volume or certificate not found for certificate ID {input.CertificateId}.");
                        }

                        // Deduct used volume from certificate
                        certificate.CurrentVolume -= input.Amount;
                        _context.Certificates.Update(certificate);
                        totalInputVolume += input.Amount;

                        // Create new transform event for each ceartificate
                        var transformEvent = new TransformEvent
                        {
                            UserId = deviceOwnerId,
                            BundleId = bundleId,
                            TransformedVolume = input.Amount,
                            TransformationTimestamp = DateTime.UtcNow,
                            RootCertificateId = await GetRootCertificateId(input.CertificateId),
                            NewCertificateId = null
                        };
                        await _context.TransformEvents.AddAsync(transformEvent);
                    }
                    await _context.SaveChangesAsync();

                    // Calculate energy production and loss
                    var energyTransformed = totalInputVolume * (transformRequest.Efficiency / 100);
                    var energyLost = totalInputVolume - energyTransformed;

                    // Create energy production request
                    var productionRequest = new EnergyProductionDto
                    {
                        ProductionTime = transformRequest.ProductionTime,
                        AmountWh = energyTransformed,
                        DeviceId = transformRequest.DeviceId
                    };
                    var result = await _productionService.CreateEnergyProduction(productionRequest, true, "TRANSFORM");

                    // Update transform events with the created certificate ID
                    var transformEvents = await _context.TransformEvents.Where(te => te.BundleId == bundleId).ToListAsync();
                    transformEvents.ForEach(te => te.NewCertificateId = result.Item2.Id);
                    _context.UpdateRange(transformEvents);

                    // Log energy loss as system user
                    var lossEvent = new TransformEvent
                    {
                        UserId = 0,
                        BundleId = bundleId,
                        TransformedVolume = energyLost,
                        TransformationTimestamp = DateTime.UtcNow
                    };
                    await _context.TransformEvents.AddAsync(lossEvent);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { EnergyProduction = result.Item1, Certificate = result.Item2 });
                }
                catch (Exception ex)
                {
                    //If error, rollback
                    await transaction.RollbackAsync();
                    var baseException = ex.GetBaseException();
                    _logger.LogError(baseException, "Error during transformation: {Message}", baseException.Message);
                    return StatusCode(500, $"Internal server error during transformation: {baseException.Message}");
                }
            }
        }

        // Helper method to find root certificate ID from a chain of certificates
        private async Task<int> GetRootCertificateId(int certificateId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.ParentCertificate)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            // Traverse up the certificate chain to find root
            while (certificate?.ParentCertificate != null)
            {
                certificate = certificate.ParentCertificate;
            }

            return certificate?.Id ?? certificateId;
        }
    }
}
