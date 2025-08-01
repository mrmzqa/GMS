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

        // DbSets go here
        public DbSet<FileItem> Files { get; set; }
         public DbSet<Main> mains { get; set; }
        
    }
}
