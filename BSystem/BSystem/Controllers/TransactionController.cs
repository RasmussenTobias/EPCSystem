using BSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace BSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly DataContext _context;

        public TransactionController(DataContext context)
        {
            _context = context;
        }

        // POST: api/Transaction
        [HttpPost]
        public IActionResult CreateTransaction([FromBody] Transaction transaction)
        {
            var fromAccount = _context.Accounts.FirstOrDefault(a => a.Id == transaction.FromAccountId);
            var toAccount = _context.Accounts.FirstOrDefault(a => a.Id == transaction.ToAccountId);

            if (fromAccount == null || toAccount == null)
            {
                return NotFound("One or both accounts not found.");
            }

            if (fromAccount.Balance < transaction.Amount)
            {
                return BadRequest("Insufficient funds.");
            }

            fromAccount.Balance -= transaction.Amount;
            toAccount.Balance += transaction.Amount;

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Ok(transaction);
        }

        // GET: api/Transaction/Account/5
        [HttpGet("Account/{accountId}")]
        public IActionResult GetTransactionsByAccountId(int accountId)
        {
            var transactions = _context.Transactions
                                       .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                                       .ToList();
            return Ok(transactions);
        }
    }
}
