using Microsoft.EntityFrameworkCore;

namespace GMSApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Add your DbSet<TEntity> properties here, e.g.:
        // public DbSet<User> Users { get; set; }
    }
}