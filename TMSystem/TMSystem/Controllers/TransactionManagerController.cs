using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TMSystem.Models;

namespace TMSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionManagerController : ControllerBase
    {
        private readonly TmsContext _context;

        public TransactionManagerController(TmsContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> HandleTradeRequest([FromBody] TransactionRequest tradeRequest)
        {
            try
            {
                // Log initiation of transaction
                LogTransaction(tradeRequest.TransactionId, "initiate", "Transaction initiated.");

                // Initiate 2-phase commit protocol
                var precommitResult = await PreCommitToRMS(tradeRequest);

                if (precommitResult.IsSuccessful)
                {
                    // Coordinate transaction between EPS and BS
                    var transactionResult = await CoordinateTransaction(tradeRequest);

                    if (transactionResult.IsSuccessful)
                    {
                        // Log successful transaction
                        LogTransaction(tradeRequest.TransactionId, "commit", "Transaction committed successfully.");

                        return Ok(new TransactionStatus
                        {
                            TransactionId = tradeRequest.TransactionId,
                            IsSuccessful = true,
                            Message = "Trade executed successfully."
                        });
                    }
                    else
                    {
                        // Log transaction failure
                        LogTransaction(tradeRequest.TransactionId, "rollback", "Transaction rolled back due to failure.");

                        return StatusCode(500, new TransactionStatus
                        {
                            TransactionId = tradeRequest.TransactionId,
                            IsSuccessful = false,
                            Message = "Transaction failed. Rollback initiated."
                        });
                    }
                }
                else
                {
                    // Log pre-commitment failure
                    LogTransaction(tradeRequest.TransactionId, "rollback", "Pre-commitment failed. Transaction aborted.");

                    return StatusCode(500, new TransactionStatus
                    {
                        TransactionId = tradeRequest.TransactionId,
                        IsSuccessful = false,
                        Message = "Pre-commitment failed. Transaction aborted."
                    });
                }
            }
            catch (Exception ex)
            {
                // Log and handle unexpected errors
                LogTransaction(tradeRequest.TransactionId, "rollback", $"An error occurred: {ex.Message}");

                return StatusCode(500, new TransactionStatus
                {
                    TransactionId = tradeRequest.TransactionId,
                    IsSuccessful = false,
                    Message = "An error occurred while processing the trade request."
                });
            }
        }

        private async Task<TransactionStatus> PreCommitToRMS(TransactionRequest tradeRequest)
        {
            // Implement pre-commit logic and communicate with RMS
            // Log pre-commit action
            LogTransaction(tradeRequest.TransactionId, "pre-commit", "Pre-commitment request sent to RMS.");

            // Simulate pre-commit success
            await Task.Delay(1000); // Simulating delay for communication
            return new TransactionStatus
            {
                TransactionId = tradeRequest.TransactionId,
                IsSuccessful = true,
                Message = "Pre-commitment successful."
            };
        }

        private async Task<TransactionStatus> CoordinateTransaction(TransactionRequest tradeRequest)
        {
            // Implement transaction coordination logic with EPS and BS
            // Log transaction coordination action
            LogTransaction(tradeRequest.TransactionId, "coordinate", "Transaction coordination initiated.");

            // Simulate transaction success
            await Task.Delay(1000); // Simulating delay for coordination
            return new TransactionStatus
            {
                TransactionId = tradeRequest.TransactionId,
                IsSuccessful = true,
                Message = "Transaction coordination successful."
            };
        }

        private void LogTransaction(Guid transactionId, string action, string details)
        {
            // Log transaction activity to the database
            _context.TransactionRecords.Add(new TransactionLog
            {
                TransactionId = transactionId,
                Timestamp = DateTime.UtcNow,
                Action = action,
                Details = details
            });
            _context.SaveChanges();
        }
    }
}
