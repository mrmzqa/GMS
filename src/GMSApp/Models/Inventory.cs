using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Mainid { get; set; }

        public Main Main { get; set; }

        public class Stock
        {
            public int Id { get; set; }

            public int StockId { get; set; }

            public InventoryItem Item { get; set; }

            public InventoryCategory Category { get; set; }

        }

        public class InventoryItem
        {
            public int Id { get; set; }
            public string ItemName { get; set; }
            public Quantity Quantity { get; set; }
            public InventoryCategory Category { get; set; }
            public decimal Price { get; set; }
            public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
            public Inventory Inventory { get; set; }
        }
        public class InventoryCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        }

        public class Quantity
        {
            public int Id { get; set; }
            public int Amount { get; set; }
            public string Unit { get; set; }
        }

        public class InventoryTransaction
        {
            public int Id { get; set; }
            public InventoryItem Item { get; set; }
            public int Quantity { get; set; }
            public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
            public Transaction TransactionType { get; set; }
            public string Remarks { get; set; }
        }

        public class Transaction
        {
            public int Id { get; set; }
            public InventoryItem Item { get; set; }
            public int Quantity { get; set; }
            public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
            public string TransactionType { get; set; } 
            public string Remarks { get; set; }

        }

        public class InventoryReport
        {
            public int Id { get; set; }
            public DateTime ReportDate { get; set; } = DateTime.UtcNow;
            public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
            public decimal TotalValue { get; set; }
            public string Remarks { get; set; }
        }
        public class InventorySearch
        {
            public string ItemName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public string Remarks { get; set; }
        }
    }

  
}
