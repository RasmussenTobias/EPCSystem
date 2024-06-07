using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;
using EPCSystemAPI.Services;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransformController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransformController> _logger;
        private readonly ElectricityProductionService _productionService;

        // Dependency constructor
        public TransformController(ApplicationDbContext context, ILogger<TransformController> logger, ElectricityProductionService productionService)
        {
            _context = context;
            _logger = logger;
            _productionService = productionService;
        }

        //Post endpoint for transformation of certificate
        [HttpPost("transform")]
        public async Task<IActionResult> TransformCertificate([FromBody] TransformRequestDto transformRequest)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Retrieve the device owner
                    var device = await _context.Devices.FindAsync(transformRequest.DeviceId);
                    if (device == null)
                    {
                        _logger.LogError($"Device with ID {transformRequest.DeviceId} not found.");
                        return NotFound($"Device with ID {transformRequest.DeviceId} not found.");
                    }

                    int deviceOwnerId = device.UserId;

                    // Check if the device owner has the required input certificates and update their volumes
                    foreach (var input in transformRequest.Inputs)
                    {
                        var certificate = await _context.Certificates
                            .FirstOrDefaultAsync(c => c.Id == input.CertificateId && c.UserId == deviceOwnerId);

                        if (certificate == null || certificate.CurrentVolume < input.Amount)
                        {
                            _logger.LogError($"Insufficient volume or certificate not found for certificate ID {input.CertificateId}.");
                            return BadRequest($"Insufficient volume or certificate not found for certificate ID {input.CertificateId}.");
                        }

                        // Withdraw the amount from the certificate
                        certificate.CurrentVolume -= input.Amount;
                        _context.Certificates.Update(certificate);
                    }

                    await _context.SaveChangesAsync();

                    // Use the ElectricityProductionService to create a new production with a TRANSFORM event type
                    var productionRequest = new ElectricityProductionDto
                    {
                        ProductionTime = transformRequest.ProductionTime,
                        AmountWh = transformRequest.AmountWh,
                        DeviceId = transformRequest.DeviceId
                    };

                    var result = await _productionService.CreateElectricityProduction(productionRequest, true, "TRANSFORM");
                    var newProduction = result.Item1;
                    var newCertificate = result.Item2;

                    // Generate a new BundleId
                    var bundleId = await _context.TransformEvents.MaxAsync(te => (int?)te.BundleId) ?? 0;
                    bundleId++;

                    // Create transform events for each input certificate
                    foreach (var input in transformRequest.Inputs)
                    {
                        var transformEvent = new TransformEvent
                        {
                            UserId = deviceOwnerId,
                            BundleId = bundleId,
                            TransformedVolume = input.Amount,
                            TransformationTimestamp = DateTime.UtcNow,
                            RootCertificateId = await GetRootCertificateId(input.CertificateId),
                            NewCertificateId = newCertificate.Id // Set NewCertificateId
                        };
                        await _context.TransformEvents.AddAsync(transformEvent);
                    }

                    await _context.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();
                    return Ok(new { ElectricityProduction = newProduction, Certificate = newCertificate });
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

        // Method to determine the root certificate ID
        private async Task<int> GetRootCertificateId(int certificateId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.ParentCertificate)
                .FirstOrDefaultAsync(c => c.Id == certificateId);

            while (certificate?.ParentCertificate != null)
            {
                certificate = await _context.Certificates
                    .Include(c => c.ParentCertificate)
                    .FirstOrDefaultAsync(c => c.Id == certificate.ParentCertificateId);
            }

            return certificate?.Id ?? certificateId;
        }
    }
}