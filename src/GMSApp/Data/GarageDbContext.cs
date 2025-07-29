using GarageApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GarageApp.Data
{
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<ServiceJob> ServiceJobs => Set<ServiceJob>();
        public DbSet<GarageWorker> GarageWorkers => Set<GarageWorker>();
        public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
        public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();
        public DbSet<VehicleMakeModel> VehicleMakeModels => Set<VehicleMakeModel>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(12);
                property.SetScale(2);
            }

            // Seed Super Admin user
            var superAdminPassword = "SuperSecret123!";
            var superAdminPasswordHash = ComputeSha256Hash(superAdminPassword);

            var superAdminUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "superadmin",
                PasswordHash = superAdminPasswordHash,
                Role = UserRole.SuperAdmin,
                IsActive = true
            };

            modelBuilder.Entity<User>().HasData(superAdminUser);
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = SHA256.Create())
            {
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach(var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}