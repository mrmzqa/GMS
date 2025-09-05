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
    public string Unit { get; set; } = "pcs";

    // Pricing
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Currency { get; set; } = "QAR";

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
    public JobOrder? JobOrder { get; set; }

    public string Notes { get; set; } = string.Empty;
}
public enum StockTransactionType
{
    Purchase = 1,
    JobUsage = 2,
    Adjustment = 3,
    Return = 4
}