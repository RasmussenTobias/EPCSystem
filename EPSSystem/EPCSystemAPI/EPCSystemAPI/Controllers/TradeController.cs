using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EPCSystemAPI.models;
using Microsoft.Extensions.Logging;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TradeController> _logger;

        public TradeController(ApplicationDbContext context, ILogger<TradeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> TradeCertificate([FromBody] TradeCertificateDto tradeDto)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var certTransfer in tradeDto.Transfers)
                    {
                        var certificate = await _context.Certificates
                            .Include(c => c.ElectricityProduction)
                            .FirstOrDefaultAsync(c => c.Id == certTransfer.CertificateId);

                        if (certificate == null)
                        {
                            return NotFound($"Certificate with ID {certTransfer.CertificateId} not found");
                        }

                        if (certificate.UserId != tradeDto.FromUserId)
                        {
                            return BadRequest($"User {tradeDto.FromUserId} does not own certificate ID {certTransfer.CertificateId}");
                        }

                        var userBalances = await _context.UserBalanceView
                            .Where(ub => ub.UserId == tradeDto.FromUserId)
                            .ToListAsync();

                        decimal totalAvailable = userBalances.Sum(ub => ub.TotalTransactionAmount);
                        if (totalAvailable < certTransfer.Amount)
                        {
                            return BadRequest($"Insufficient balance for certificate ID {certTransfer.CertificateId}");
                        }

                        // Opret en post i _TransferEvents-tabellen
                        var transferEvent = new TransferEvent
                        {                            
                            FromUserId = tradeDto.FromUserId,
                            ToUserId = tradeDto.ToUserId,
                            Volume = certTransfer.Amount
                        };
                        _context.Add(transferEvent);

                        // Save changes to the database to generate IDs for _TransferEvents
                        await _context.SaveChangesAsync();

                        // Nu har vi det genererede ID fra _TransferEvents-tabellen
                        var transferEventId = transferEvent.Id;

                        // Opret en post i TransferLedger-tabellen
                        var deductionLedgerEntry = new TransferLedger
                        {
                            EventType = "DEDUCTION",
                            TransactionDate = DateTime.Now,
                            ElectricityProductionId = certificate.ElectricityProductionId,
                            Volume = -certTransfer.Amount,
                            TransferEventId = transferEventId
                        };
                        _context.Add(deductionLedgerEntry);

                        // Opret en post i TransferLedger-tabellen for increase (hvis relevant)
                        var increaseLedgerEntry = new TransferLedger
                        {
                            EventType = "INCREASE",
                            TransactionDate = DateTime.Now,
                            ElectricityProductionId = certificate.ElectricityProductionId,
                            Volume = certTransfer.Amount,
                            TransferEventId = transferEventId
                        };
                        _context.Add(increaseLedgerEntry);

                        // Update the certificate's owner
                        certificate.UserId = tradeDto.ToUserId;

                        // Hvis overførslen ikke dækker det fulde volumen, opret et nyt certifikat for modtageren
                        if (certTransfer.Amount < certificate.volume)
                        {
                            // Fratræk det overførte volumen fra det eksisterende certifikat
                            certificate.volume -= certTransfer.Amount;

                            // Opret et nyt certifikat for modtageren med det overførte volumen
                            var newCertificate = new Certificate
                            {
                                UserId = tradeDto.ToUserId,
                                ElectricityProductionId = certificate.ElectricityProductionId,
                                CreatedAt = DateTime.Now,
                                volume = certTransfer.Amount
                            };
                            _context.Add(newCertificate);
                        }
                        else
                        {
                            // Hvis overførslen dækker det fulde volumen, fjern det eksisterende certifikat
                            _context.Remove(certificate);
                        }
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok("Certificates traded successfully");
                }
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
