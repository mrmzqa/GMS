using GMSApp.Models.job;
using GMSApp.Models.purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.inventory
{
    public class StockTransaction
    {
        public int Id { get; set; }

        public int InventoryItemId { get; set; }
        public InventoryItem InventoryItem { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public StockTransactionType TransactionType { get; set; } // Purchase, JobUsage, Adjustment, Return
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Snapshot at time of transaction

        // References
        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int? JobOrderId { get; set; }
        public Joborder? JobOrder { get; set; }

        public string Notes { get; set; } = string.Empty;
    }
}
