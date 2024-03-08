using Microsoft.EntityFrameworkCore;
using ReservationApi.Models;

namespace ReservationApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Provider> Providers { get; set; }

        // do it in Program.cs and appsettings.json
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite("Data Source=reservation.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Provider>()
                .HasMany(p => p.Availabilities)
                .WithOne(a => a.Provider)
                .HasForeignKey(a => a.ProviderId);

            modelBuilder.Entity<Availability>()
                .HasMany(a => a.Appointments)
                .WithOne(app => app.Availability)
                .HasForeignKey(app => app.AvailabilityId);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Appointments)
                .WithOne(app => app.Client)
                .HasForeignKey(app => app.ClientId);

            // although Id is set to Key with auto-increment, need to set Id with HasData, or exception will be thrown
            modelBuilder.Entity<Provider>()
                .HasData(
                    new Provider { Id = 1, Name = "Li" },
                    new Provider { Id = 2, Name = "Jekyll" },
                    new Provider { Id = 3, Name = "Meds" });

            modelBuilder.Entity<Client>()
                .HasData(
                    new Client { Id = 1, Name = "C1" },
                    new Client { Id = 2, Name = "C2" },
                    new Client { Id = 3, Name = "C3" });

        }

    }
}