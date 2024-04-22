using EPCSystemAPI.models;
using Microsoft.EntityFrameworkCore;

namespace EPCSystemAPI
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<ElectricityProduction> ElectricityProductions { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<TransferEvent> TransferEvents { get; set; }
        public DbSet<TransformEvent> TransformEvents { get; set; }
        public DbSet<ProduceEvent> ProduceEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToTable("users");

            modelBuilder.Entity<Device>()
                .ToTable("devices")
                .HasOne<User>(d => d.User)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElectricityProduction>()
                .ToTable("electricity_production");

            modelBuilder.Entity<Certificate>()
                .ToTable("certificates")
                .HasOne<User>()
                .WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ledger>()
                .ToTable("ledger");

            modelBuilder.Entity<TransferEvent>()
                .ToTable("transfer_events");

            modelBuilder.Entity<TransformEvent>()
                .ToTable("transform_events");

            modelBuilder.Entity<ProduceEvent>()
                .ToTable("produce_events");
        }
    }
}
