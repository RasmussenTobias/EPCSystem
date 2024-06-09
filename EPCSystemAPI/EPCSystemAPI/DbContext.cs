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

        //Setting the tables
        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<ElectricityProduction> Electricity_Production { get; set; }
        public DbSet<Certificate> Certificates { get; set; }         
        public DbSet<Event> Events { get; set; }
        public DbSet<TransferEvent> TransferEvents { get; set; }
        public DbSet<TransformEvent> TransformEvents { get; set; }
        public DbSet<TradeEvent> TradeEvents { get; set; }
        public DbSet<ProduceEvent> ProduceEvents { get; set; }
        public DbSet<PendingTrade> PendingTrades { get; set; }
        public DbSet<UserBalanceView> UserBalanceView { get; set; }
        public DbSet<UserEventView> UserEventViews { get; set; }


        //Relationships for the database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Devices)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Certificates)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Device>()
                .HasMany(d => d.ElectricityProductions)
                .WithOne(e => e.Device)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElectricityProduction>()
                .HasOne(e => e.Device)
                .WithMany(d => d.ElectricityProductions)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasOne<ElectricityProduction>(c => c.ElectricityProduction)
                .WithMany()  
                .HasForeignKey(c => c.ElectricityProductionId)
                .OnDelete(DeleteBehavior.Restrict);  

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.ParentCertificate)
                .WithMany(c => c.ChildCertificates)
                .HasForeignKey(c => c.ParentCertificateId);

            modelBuilder.Entity<TransformEvent>()
                .HasOne(te => te.RootCertificate)
                .WithMany()
                .HasForeignKey(te => te.RootCertificateId);

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

            modelBuilder.Entity<UserBalanceView>().ToView("UserBalanceView");
            modelBuilder.Entity<UserBalanceView>().HasNoKey();

            modelBuilder.Entity<UserEventView>().HasNoKey().ToView("UserEventView");
        }

    }
}
