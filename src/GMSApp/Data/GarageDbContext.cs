using GMSApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GMSApp.Data
{
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options)
            : base(options)
        {
        }

        // DbSets for each table
        public DbSet<Vehicle> Vehicles { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example: Composite key (if needed)
            // modelBuilder.Entity<VehicleMakeModel>()
            //     .HasKey(v => new { v.Make, v.Model });

            // Example: Relationship setup (if any)
            // modelBuilder.Entity<ServiceJob>()
            //     .HasOne(j => j.Vehicle)
            //     .WithMany()
            //     .HasForeignKey(j => j.VehicleId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
