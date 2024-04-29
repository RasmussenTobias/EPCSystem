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
                // Retrieve all trades where the user is either the sender or recipient
                var allTradeRequests = await _context.PendingTrades
                    .Where(pt => pt.FromUserId == userId || pt.ToUserId == userId)
                    .ToListAsync();

                // Group the trades by bundleId
                var groupedTradeRequests = allTradeRequests
                    .GroupBy(pt => pt.BundleId)
                    .ToList();

                // Filter the groups to include only those where the user is the sender according to the first entry
                var sentTradeRequests = groupedTradeRequests
                    .Where(group => group.First().FromUserId == userId)
                    .Select(group => new
                    {
                        BulkId = group.Key,
                        OfferedCertificates = group.First().FromUserId == userId
                            ? group.Select(pt => new
                            {
                                ElectricityProductionId = pt.ElectricityProductionId,
                                Amount = pt.Volume
                            }).ToList<object>() // Explicit conversion to List<object>
                            : new List<object>(),
                        RequestedCertificates = group.First().ToUserId == userId
                            ? group.Select(pt => new
                            {
                                ElectricityProductionId = pt.ElectricityProductionId,
                                Amount = pt.Volume
                            }).ToList<object>() // Explicit conversion to List<object>
                            : new List<object>()
                    }).ToList();

                return Ok(sentTradeRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving sent trade requests: {Message}", ex.InnerException?.Message ?? ex.Message);
                return StatusCode(500, $"An error occurred while retrieving sent trade requests: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        [HttpGet("ReceivedTradeRequests/{userId}")]
        public async Task<IActionResult> GetReceivedTradeRequests(int userId)
        {
            try
            {
                // Retrieve all trades where the user is either the sender or recipient
                var allTradeRequests = await _context.PendingTrades
                    .Where(pt => pt.FromUserId == userId || pt.ToUserId == userId)
                    .ToListAsync();

                // Group the trades by bundleId
                var groupedTradeRequests = allTradeRequests
                    .GroupBy(pt => pt.BundleId)
                    .ToList();

                // Filter the groups to include only those where the user is the recipient according to the first entry
                var receivedTradeRequests = groupedTradeRequests
                    .Where(group => group.First().ToUserId == userId)
                    .Select(group => new
                    {
                        BulkId = group.Key,
                        OfferedCertificates = group.First().FromUserId == userId
                            ? group.Select(pt => new
                            {
                                ElectricityProductionId = pt.ElectricityProductionId,
                                Amount = pt.Volume
                            }).ToList<object>() // Explicit conversion to List<object>
                            : new List<object>(),
                        RequestedCertificates = group.First().ToUserId == userId
                            ? group.Select(pt => new
                            {
                                ElectricityProductionId = pt.ElectricityProductionId,
                                Amount = pt.Volume
                            }).ToList<object>() // Explicit conversion to List<object>
                            : new List<object>()
                    }).ToList();

                return Ok(receivedTradeRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving received trade requests: {Message}", ex.InnerException?.Message ?? ex.Message);
                return StatusCode(500, $"An error occurred while retrieving received trade requests: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

    }
}
