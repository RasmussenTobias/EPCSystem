using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using EPCSystemAPI.models;
using EPCSystemAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradeController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ILogger<TradeController> _logger;
        private readonly ITransactionManagementService _tmsService;
        private readonly ApplicationDbContext _context;

        // Dependency constructor
        public TradeController(ILogger<TradeController> logger, ITransactionManagementService tmsService, ApplicationDbContext context)
        {
            _logger = logger;
            _tmsService = tmsService;
            _context = context;
        }

        //Post endpoint to initiate a trade
        [HttpPost("initiateTrade")]
        public async Task<ActionResult<TradeResponse>> InitiateTrade([FromBody] TradeRequestDto tradeRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate ToUserId
                var toUser = await _context.Users.FindAsync(tradeRequest.ToUserId);
                if (toUser == null)
                {
                    return BadRequest(new TradeResponse
                    {
                        IsSuccess = false,
                        Message = "ToUserId is invalid."
                    });
                }

                // Validation phase
                var validation = await ValidateTradeRequest(tradeRequest);
                if (!validation.IsValid)
                {
                    return BadRequest(new TradeResponse
                    {
                        IsSuccess = false,
                        Message = validation.ErrorMessage
                    });
                }

                // Pre-commit phase
                var preCommitResponse = await _tmsService.SendPreCommit(tradeRequest);
                if (!preCommitResponse.IsSuccess)
                {
                    return BadRequest(new TradeResponse
                    {
                        IsSuccess = false,
                        Message = preCommitResponse.Message
                    });
                }

                // Determine the trade response based on TradeResponseType
                if (tradeRequest.TradeResponseType == TradeResponseType.Abort)
                {
                    // Abort the transaction
                    await _tmsService.AbortTransaction(tradeRequest);
                    return BadRequest(new TradeResponse
                    {
                        IsSuccess = false,
                        Message = "Trade aborted as requested."
                    });
                }

                // Commit phase
                var commitResponse = await _tmsService.CommitTransaction(tradeRequest);
                if (!commitResponse.IsSuccess)
                {
                    // Abort if commit fails
                    await _tmsService.AbortTransaction(tradeRequest);
                    return BadRequest(new TradeResponse
                    {
                        IsSuccess = false,
                        Message = commitResponse.Message
                    });
                }

                // If commit successful, execute the transfer
                var updatedCertificates = new List<CertificateDto>();
                var totalVolume = 0m;
                int currency = 0;
                foreach (var offeredCertificate in tradeRequest.OfferedCertificates)
                {
                    // Find the user's certificate
                    var userCertificate = await _context.Certificates
                        .FirstOrDefaultAsync(c => c.Id == offeredCertificate.CertificateId && c.UserId == tradeRequest.FromUserId);

                    if (userCertificate == null || userCertificate.CurrentVolume < offeredCertificate.Amount)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new TradeResponse
                        {
                            IsSuccess = false,
                            Message = "Insufficient certificate volume for the trade."
                        });
                    }

                    // Subtract the amount from the FromUser's certificate
                    userCertificate.CurrentVolume -= offeredCertificate.Amount;
                    totalVolume += offeredCertificate.Amount;
                    currency = userCertificate.EnergyProductionId; // Set currency to EnergyProductionId

                    // Create a new certificate for the ToUser
                    var newCertificate = new Certificate
                    {
                        UserId = tradeRequest.ToUserId,
                        EnergyProductionId = userCertificate.EnergyProductionId,
                        CreatedAt = DateTime.Now,
                        Volume = offeredCertificate.Amount,
                        CurrentVolume = offeredCertificate.Amount
                    };
                    _context.Certificates.Add(newCertificate);

                    // Add to the response list
                    updatedCertificates.Add(new CertificateDto
                    {
                        UserId = tradeRequest.ToUserId,
                        EnergyProductionId = userCertificate.EnergyProductionId,
                        Amount = offeredCertificate.Amount
                    });
                }

                // Add a trade event
                var tradeEvent = new TradeEvent
                {
                    FromUserId = tradeRequest.FromUserId,
                    ToUserId = tradeRequest.ToUserId,
                    Volume = totalVolume,
                    Currency = currency
                };
                _context.TradeEvents.Add(tradeEvent);
                await _context.SaveChangesAsync();

                // Add an event
                var eventEntry = new Event
                {
                    Event_Type = "Trade",
                    Reference_Id = tradeEvent.Id,
                    User_Id = tradeRequest.FromUserId,
                    Timestamp = DateTime.Now
                };
                _context.Events.Add(eventEntry);

                // Save changes and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new TradeResponse
                {
                    IsSuccess = true,
                    Message = "Trade committed successfully."
                });
            }
            //If error, rollback
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error initiating trade: {Message}", ex.Message);
                return StatusCode(500, new TradeResponse
                {
                    IsSuccess = false,
                    Message = "Internal server error while initiating trade."
                });
            }
        }

        //Validate the traderequest
        private async Task<ValidationResult> ValidateTradeRequest(TradeRequestDto tradeRequest)
        {
            foreach (var offeredCertificate in tradeRequest.OfferedCertificates)
            {
                var userCertificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.Id == offeredCertificate.CertificateId && c.UserId == tradeRequest.FromUserId);

                if (userCertificate == null || userCertificate.CurrentVolume < offeredCertificate.Amount)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Insufficient certificate volume for CertificateId {offeredCertificate.CertificateId}."
                    };
                }
            }

            return new ValidationResult { IsValid = true };
        }
    }
}
