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
    [Route("[controller]")]
    public class CertificatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ApplicationDbContext context, ILogger<CertificatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Certificate>>> GetCertificatesByUserId(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(certificates);
        }
    }
}