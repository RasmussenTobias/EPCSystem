using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransferController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransferController> _logger;
        
        // Dependency constructor
        public TransferController(ApplicationDbContext context, ILogger<TransferController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteTransfer([FromBody] TradeCertificateDto tradeDto)
        {
            // Validate ToUserId
            var toUser = await _context.Users.FindAsync(tradeDto.ToUserId);
            if (toUser == null)
            {
                return BadRequest($"ToUserId {tradeDto.ToUserId} is invalid.");
            }

            // Find the maximum bundle ID in the TransferEvents table
            var maxBundleId = await _context.TransferEvents
                .MaxAsync(te => (int?)te.BundleId); // Use nullable int to handle null values

            var bundleId = maxBundleId.HasValue ? maxBundleId.Value + 1 : 0; // Initialize bundle ID

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {                    
                    foreach (var certTransfer in tradeDto.Transfers)
                    {
                        //Get certificates and production data
                        var originalCertificate = await _context.Certificates
                            .Include(c => c.ElectricityProduction)
                            .FirstOrDefaultAsync(c => c.Id == certTransfer.CertificateId);

                        //Checks - insuffiecient certificate id, user do not own id, not enough volume on certificate
                        if (originalCertificate == null)
                        {
                            return NotFound($"Certificate with ID {certTransfer.CertificateId} not found");
                        }

                        if (originalCertificate.UserId != tradeDto.FromUserId)
                        {
                            return BadRequest($"User {tradeDto.FromUserId} does not own certificate ID {certTransfer.CertificateId}");
                        }

                        if (originalCertificate.CurrentVolume < certTransfer.Amount)
                        {
                            return BadRequest($"Insufficient certificate volume for transfer. Available: {originalCertificate.CurrentVolume}, Attempted to transfer: {certTransfer.Amount}");
                        }

                        //Subtract the volume from the original certificate
                        originalCertificate.CurrentVolume -= certTransfer.Amount;
                        _context.Certificates.Update(originalCertificate);

                        // Create a new certificate for the receiver with the transferred volume
                        var receiverCertificate = new Certificate
                        {
                            UserId = tradeDto.ToUserId,
                            ElectricityProductionId = originalCertificate.ElectricityProductionId,
                            CreatedAt = DateTime.Now,
                            Volume = certTransfer.Amount,
                            CurrentVolume = certTransfer.Amount
                        };
                        _context.Certificates.Add(receiverCertificate);

                        //Create event 
                        var transferEvent = new Event
                        {
                            Event_Type = "TRANSFER",
                            User_Id = tradeDto.FromUserId,
                            Timestamp = DateTime.Now
                        };
                        _context.Events.Add(transferEvent);
                        await _context.SaveChangesAsync();

                        //Create transfer event
                        var transferEventEntry = new TransferEvent
                        {
                            FromUserId = tradeDto.FromUserId,
                            ToUserId = tradeDto.ToUserId,
                            Volume = certTransfer.Amount,
                            BundleId = bundleId,
                            Electricity_Production_Id = originalCertificate.ElectricityProductionId
                        };
                        _context.TransferEvents.Add(transferEventEntry);

                        // Commit changes to get the ID of the TransferEvent
                        await _context.SaveChangesAsync();

                        // Set the Reference_Id of the Event to be the ID of the TransferEvent
                        transferEvent.Reference_Id = transferEventEntry.Id;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return Ok("Certificates traded successfully");
                }
                //If error, rollback
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while trading certificates: {Message}", ex.InnerException?.Message ?? ex.Message);
                    return StatusCode(500, $"An error occurred while trading certificates: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }
    }
}
