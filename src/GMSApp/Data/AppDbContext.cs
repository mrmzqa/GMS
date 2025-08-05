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
     
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<ItemRow> ItemRows {  get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Status> Statuses { get; set; }

    

        public DbSet<Vendor> Vendors { get; set; }

        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<Account> Accounts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
         

        }


    }
}
