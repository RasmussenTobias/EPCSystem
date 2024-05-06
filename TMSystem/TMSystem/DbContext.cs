using Microsoft.EntityFrameworkCore;
using TMSystem.Models;

namespace TMSystem
{
    public class TmsContext : DbContext
    {
        public TmsContext(DbContextOptions<TmsContext> options) : base(options) { }

        public DbSet<TransactionRecord> TransactionRecords { get; set; }
    }
}