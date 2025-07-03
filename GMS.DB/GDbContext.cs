using GMS.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace GMS.DB
{
    public class GDbContext : DbContext
    {
        public GDbContext(DbContextOptions<DbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<JobOrder> JobOrders { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<VehicleDelivery> VehicleDeliveries { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
