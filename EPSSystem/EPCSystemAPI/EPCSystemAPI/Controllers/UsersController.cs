using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EPCSystemAPI.Models;
using EPCSystemAPI.models;
using Microsoft.EntityFrameworkCore;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransferController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<TransferController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username
            }).ToList();

            return Ok(userDtos);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(UserDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostUser), new { id = user.Id }, userDto);
        }

        [HttpGet("UserBalance/{userId}")]
        public async Task<IActionResult> GetUserBalance(int userId)
        {
            try
            {
                var userBalances = await _context.UserBalanceView
                    .Where(ub => ub.UserId == userId)
                    .Join(
                        _context.Electricity_Production,
                        ub => ub.ElectricityProductionId,
                        ep => ep.Id,
                        (ub, ep) => new { UserBalance = ub, ElectricityProduction = ep }
                    )
                    .Join(
                        _context.Devices,
                        joined => joined.ElectricityProduction.DeviceId,
                        d => d.Id,
                        (joined, d) => new
                        {
                            UserId = joined.UserBalance.UserId,
                            ElectricityProductionId = joined.UserBalance.ElectricityProductionId,
                            Balance = joined.UserBalance.Balance,
                            PowerType = d.PowerType,
                            DeviceType = d.DeviceType,
                            DeviceName = d.DeviceName
                        }
                    )
                    .ToListAsync();

                if (userBalances == null || !userBalances.Any())
                {
                    return NotFound($"User balance not found for user with ID {userId}");
                }

                return Ok(userBalances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user balance: {Message}", ex.InnerException?.Message ?? ex.Message);
                return StatusCode(500, $"An error occurred while retrieving user balance: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

    }
}
