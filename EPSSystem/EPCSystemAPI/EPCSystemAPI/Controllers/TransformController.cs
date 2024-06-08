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
                    var device = await _context.Devices.FindAsync(transformRequest.DeviceId);
                    if (device == null)
                    {
                        _logger.LogError($"Device with ID {transformRequest.DeviceId} not found.");
                        return NotFound($"Device with ID {transformRequest.DeviceId} not found.");
                    }

                    int deviceOwnerId = device.UserId;
                    decimal totalInputVolume = 0;

                    foreach (var input in transformRequest.Inputs)
                    {
                        var certificate = await _context.Certificates.FirstOrDefaultAsync(c => c.Id == input.CertificateId && c.UserId == deviceOwnerId);
                        if (certificate == null || certificate.CurrentVolume < input.Amount)
                        {
                            _logger.LogError($"Insufficient volume or certificate not found for certificate ID {input.CertificateId}.");
                            return BadRequest($"Insufficient volume or certificate not found for certificate ID {input.CertificateId}.");
                        }

                        certificate.CurrentVolume -= input.Amount;
                        _context.Certificates.Update(certificate);
                        totalInputVolume += input.Amount;
                    }

                    await _context.SaveChangesAsync();

                    // Calculate transformed and lost energy based on efficiency
                    var energyTransformed = totalInputVolume * (transformRequest.Efficiency / 100);
                    var energyLost = totalInputVolume - energyTransformed;

                    var productionRequest = new ElectricityProductionDto
                    {
                        ProductionTime = transformRequest.ProductionTime,
                        AmountWh = energyTransformed,
                        DeviceId = transformRequest.DeviceId
                    };

                    var result = await _productionService.CreateElectricityProduction(productionRequest, true, "TRANSFORM");
                    var bundleId = await _context.TransformEvents.MaxAsync(te => (int?)te.BundleId) ?? 0;
                    bundleId++;

                    foreach (var input in transformRequest.Inputs)
                    {
                        var transformEvent = new TransformEvent
                        {
                            UserId = deviceOwnerId,
                            BundleId = bundleId,
                            TransformedVolume = input.Amount * (transformRequest.Efficiency / 100),
                            TransformationTimestamp = DateTime.UtcNow,
                            RootCertificateId = await GetRootCertificateId(input.CertificateId),
                            NewCertificateId = result.Item2.Id
                        };
                        await _context.TransformEvents.AddAsync(transformEvent);
                    }

                    // Add a loss event with the same BundleId
                    var lossEvent = new TransformEvent
                    {
                        UserId = 0,  // System user for lost energy
                        BundleId = bundleId,  // Same bundle as the transformation
                        TransformedVolume = energyLost,
                        TransformationTimestamp = DateTime.UtcNow,
                        RootCertificateId = null, 
                        NewCertificateId = null  
                    };
                    await _context.TransformEvents.AddAsync(lossEvent);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { ElectricityProduction = result.Item1, Certificate = result.Item2 });
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