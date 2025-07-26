using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMS25.Models;

namespace GMS25.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=WpfPosApp;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial data
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Description = "Electronic Items" },
                new Category { CategoryId = 2, Name = "Clothing", Description = "Clothing Items" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Laptop", Description = "High performance laptop", Price = 999.99m, StockQuantity = 10, CategoryId = 1 },
                new Product { ProductId = 2, Name = "Smartphone", Description = "Latest smartphone", Price = 699.99m, StockQuantity = 15, CategoryId = 1 },
                new Product { ProductId = 3, Name = "T-Shirt", Description = "Cotton t-shirt", Price = 19.99m, StockQuantity = 50, CategoryId = 2 }
            );

            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin", Password = "admin123", Role = "Admin" },
                new User { UserId = 2, Username = "cashier", Password = "cashier123", Role = "Cashier" }
            );
        }
    }
}