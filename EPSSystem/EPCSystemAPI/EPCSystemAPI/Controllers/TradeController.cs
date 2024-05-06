using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EPCSystemAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ILogger<TradeController> _logger;
        private readonly ITransactionManagementService _tmsService;

        public TradeController(ILogger<TradeController> logger, ITransactionManagementService tmsService)
        {
            _logger = logger;
            _tmsService = tmsService;
        }

        [HttpPost("initiateTrade")]
        public async Task<IActionResult> InitiateTrade([FromBody] TradeRequestDto tradeRequest)
        {
            try
            {
                var validation = await ValidateTradeRequest(tradeRequest);
                if (!validation.IsValid)
                {
                    return BadRequest(validation.ErrorMessage);
                }

                // Pre-commit phase
                var preCommitResponse = await _tmsService.SendPreCommit(tradeRequest);
                if (!preCommitResponse.IsSuccess)
                {
                    return BadRequest(preCommitResponse.Message);
                }

                // Commit phase
                var commitResponse = await _tmsService.CommitTransaction(tradeRequest);
                if (!commitResponse.IsSuccess)
                {
                    // Abort if commit fails
                    await _tmsService.AbortTransaction(tradeRequest);
                    return BadRequest(commitResponse.Message);
                }

                // If commit successful, execute the transfer
                return RedirectToAction("ExecuteTransfer", "Transfer", tradeRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating trade: {Message}", ex.Message);
                return StatusCode(500, "Internal server error while initiating trade.");
            }
        }

        private async Task<ValidationResult> ValidateTradeRequest(TradeRequestDto tradeRequest)
        {
            // Example validation logic (extend as needed)
            return new ValidationResult { IsValid = true };
        }
    }
}
