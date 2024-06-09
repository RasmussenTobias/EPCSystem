using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]  
    public class UsersController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ApplicationDbContext _context;  
        private readonly ILogger<UsersController> _logger;

        // Dependency constructor
        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //GET endpoint to get all users from the database
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

        //POST endpoint to create a new user
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(UserDto userDto)
        {
            //Username checks
            if (string.IsNullOrWhiteSpace(userDto.Username))
            {
                return BadRequest("Username cannot be empty.");
            }
            var existingUser = await _context.Users
                                             .FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists."); 
            }

            // Create a new user
            var user = new User
            {
                Username = userDto.Username
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //Respond to client
            return CreatedAtAction(nameof(PostUser), new { id = user.Id }, userDto);
        }

        //GET endpoint to retrieve balance for userid
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

                //Check userbalance
                if (userBalances == null || !userBalances.Any())
                {
                    return NotFound($"User balance not found for user with ID {userId}");
                }

                return Ok(userBalances);  
            }
            //Return error
            catch (Exception ex)
            {                
                _logger.LogError(ex, "An error occurred while retrieving user balance: {Message}", ex.InnerException?.Message ?? ex.Message);
                return StatusCode(500, $"An error occurred while retrieving user balance: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
