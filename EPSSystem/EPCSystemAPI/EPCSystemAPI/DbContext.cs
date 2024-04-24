using EPCSystemAPI.models;
using EPCSystemAPI.Models;
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
        //public DbSet<Ledger> Ledger { get; set; }
        public DbSet<ProduceLedger> produceLedger { get; set; }
        public DbSet<TransferLedger> transferLedger { get; set; }
        public DbSet<TransformLedger> transformLedger { get; set; }
        public DbSet<TransferEvent> TransferEvents { get; set; }
        public DbSet<TransformEvent> TransformEvents { get; set; }
        public DbSet<ProduceEvent> ProduceEvents { get; set; }

        public DbSet<UserBalanceView> UserBalanceView { get; set; } 

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

            modelBuilder.Entity<ProduceLedger>()
                .ToTable("ProduceLedger");

            modelBuilder.Entity<TransferLedger>()
               .ToTable("TransferLedger");

            modelBuilder.Entity<TransformLedger>()
               .ToTable("TransformLedger");

            modelBuilder.Entity<TransferEvent>()
                .ToTable("_TransferEvents");

            modelBuilder.Entity<TransformEvent>()
                .ToTable("transformevents");

            modelBuilder.Entity<ProduceEvent>()
                .ToTable("produce_events");

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.ElectricityProduction)
                .WithOne()
                .HasForeignKey<Certificate>(c => c.ElectricityProductionId);

            modelBuilder.Entity<UserBalanceView>().ToView("UserBalanceView");
            modelBuilder.Entity<UserBalanceView>().HasNoKey();
        }
    }
}
