using GMSApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GMSApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileItem> Files { get; set; }
        public DbSet<CoreMain> CoreMains { get; set; }
        public DbSet<Main> Mains { get; set; }
public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }


    }
}
