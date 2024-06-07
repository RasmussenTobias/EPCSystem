using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EPCSystemAPI.models;

namespace EPCSystemAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CertificatesController : ControllerBase
    {
        // Dependency injections for dbcontext
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CertificatesController> _logger;

        // Dependency constructor
        public CertificatesController(ApplicationDbContext context, ILogger<CertificatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET endpoint to retrieve certificates for user ID
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Certificate>>> GetCertificatesByUserId(int userId)
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {                
                return NotFound($"User with ID {userId} not found.");
            }

            // Get all certificates for user
            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .ToListAsync(); 

            // Return the list of certificates
            return Ok(certificates);
        }
    }
}
