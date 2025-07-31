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

       public DbSet<Vehicle> Vehicles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
        }
    }
}
