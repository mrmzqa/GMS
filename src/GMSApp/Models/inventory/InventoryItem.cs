using GMSApp.Models.account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.inventory
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Categorization
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;

        // Stock Info
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; } = 5;
        public Unit Unit { get; set; } = Unit.Piece;

        // Pricing
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public Currency Currency { get; set; } = Currency.QAR;

        // Vendor Reference
        public int? VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        // Tracking
        public string Location { get; set; } = string.Empty;
        public DateTime LastRestocked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
    }
}
