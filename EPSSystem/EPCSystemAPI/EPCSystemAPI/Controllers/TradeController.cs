using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EPCSystemAPI.Models;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.Models;
using EPCSystemAPI.models;

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
        public async Task<IActionResult> SendTrade([FromBody] TradeRequestDto tradeRequest)
        {
            try
            {
                // Validate the trade request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Find the maximum BulkId in the PendingTrades table
                var maxBulkId = await _context.PendingTrades
                    .MaxAsync(pt => (int?)pt.BundleId); // Use nullable int to handle null values

                var bulkId = 0; // Initialize bulk ID with a default value

                // If there are existing bulk IDs, increment the maximum value by 1
                if (maxBulkId.HasValue)
                {
                    bulkId = maxBulkId.Value + 1;
                }

                // Check if the users exist
                var fromUser = await _context.Users.FindAsync(tradeRequest.FromUserId);
                var toUser = await _context.Users.FindAsync(tradeRequest.ToUserId);

                if (fromUser == null || toUser == null)
                {
                    return NotFound("One or both users not found.");
                }

                // Create pending trades for each offer and request
                foreach (var offer in tradeRequest.OfferedCertificates)
                {
                    var pendingTradeOffer = new PendingTrade
                    {
                        BundleId = bulkId,
                        FromUserId = tradeRequest.FromUserId,
                        ToUserId = tradeRequest.ToUserId,
                        Volume = offer.Amount,
                        ElectricityProductionId = offer.ElectricityProductionId
                    };

                    _context.PendingTrades.Add(pendingTradeOffer);
                }

                foreach (var request in tradeRequest.RequestedCertificates)
                {
                    var pendingTradeRequest = new PendingTrade
                    {
                        BundleId = bulkId,
                        FromUserId = tradeRequest.ToUserId,
                        ToUserId = tradeRequest.FromUserId,
                        Volume = request.Amount,
                        ElectricityProductionId = request.ElectricityProductionId
                    };

                    _context.PendingTrades.Add(pendingTradeRequest);
                }

                await _context.SaveChangesAsync();

                return Ok("Trade request sent successfully.");
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while processing the trade request: {Message}", ex.InnerException?.Message ?? ex.Message);
                return StatusCode(500, $"An error occurred while processing the trade request: {ex.InnerException?.Message ?? ex.Message}");
            }
        }



        [HttpGet("SentTradeRequests/{userId}")]
        public async Task<IActionResult> GetSentTradeRequests(int userId)
        {
            try
            {
                var sentTradeRequests = await _context.PendingTrades
                    .Where(pt => pt.FromUserId == userId) // Sent by the user
                    .GroupBy(pt => pt.BundleId) // Group by bulkId
                    .Select(group => new
                    {
                        BulkId = group.Key,
                        OfferedCertificates = group.Select(pt => new
                        {
                            ElectricityProductionId = pt.ElectricityProductionId,
                            Amount = pt.Volume
                        }).ToList(),
                        RequestedCertificates = _context.PendingTrades
                            .Where(pt => pt.BundleId == group.Key && pt.ToUserId == userId && pt.FromUserId != userId) // Requested by the user but not from themselves
                            .Select(pt => new
                            {
                                ElectricityProductionId = pt.ElectricityProductionId,
                                Amount = pt.Volume
                            }).ToList()
                    }).ToListAsync();

                return Ok(sentTradeRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving sent trade requests: {Message}", ex.InnerException?.Message ?? ex.Message);
                return StatusCode(500, $"An error occurred while retrieving sent trade requests: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


    }
}
