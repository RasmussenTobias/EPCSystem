using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPCSystemAPI.Models;  // Ensure correct namespace
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserEventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserEventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<object>> GetUserEvents(int userId)
        {
            var userEvents = await _context.UserEventViews
                .Where(ue => ue.UserId == userId)
                .OrderBy(ue => ue.Timestamp)
                .ToListAsync();

            var balance = userEvents.Sum(ue => ue.Value);

            return Ok(new
            {
                Balance = balance,
                UserEvents = userEvents
            });
        }
    }

}