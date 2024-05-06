using BSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;

        public AccountController(DataContext context)
        {
            _context = context;
        }

        // POST: api/Account
        [HttpPost]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            _context.Accounts.Add(account);
            _context.SaveChanges();
            return CreatedAtAction("GetAccount", new { id = account.Id }, account);
        }

        // GET: api/Account/5
        [HttpGet("{id}")]
        public IActionResult GetAccount(int id)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Id == id);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }
    }
}
