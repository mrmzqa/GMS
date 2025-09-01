using GMSApp.Models;
using GMSApp.Models.invoice;
using GMSApp.Models.job;
using GMSApp.Models.purchase;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


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
        public DbSet<ItemRow> ItemRows {  get; set; }
        public DbSet<Joborder> Joborders { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Invoiceline> InvoiceLines { get; set; }
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PurchaseOrder ↔ Lines
            
            // Vendor ↔ PurchaseOrders
            modelBuilder.Entity<PurchaseOrder>()
      .HasOne(po => po.Vendor)
      .WithMany() // or .WithMany(v => v.PurchaseOrders) if you define navigation
      .HasForeignKey(po => po.VendorId)
      .OnDelete(DeleteBehavior.SetNull);

            // Invoice ↔ Lines
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Lines)
                .WithOne(l => l.Invoice)
                .HasForeignKey(l => l.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vendor ↔ Invoices
            modelBuilder.Entity<Vendor>()
                .HasMany(v => v.Invoices)
                .WithOne(i => i.Vendor)
                .HasForeignKey(i => i.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Vendor ↔ PaymentReceipts
            modelBuilder.Entity<Vendor>()
                .HasMany(v => v.PaymentReceipts)
                .WithOne(r => r.Vendor)
                .HasForeignKey(r => r.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice ↔ PaymentReceipts
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.PaymentReceipts)
                .WithOne(r => r.Invoice)
                .HasForeignKey(r => r.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }*/
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

           
        }


    }
}
