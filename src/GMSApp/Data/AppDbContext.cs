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
         public DbSet<CoreMain> CoreMains { get; set; }
public DbSet<Main> Mains { get; set; }

public DbSet<Labels> Labels { get; set; }
public DbSet<QuotationLabel> QuotationLabels { get; set; }
public DbSet<InventoryLabel> InventoryLabels { get; set; }
public DbSet<ProductLabel> ProductLabels { get; set; }
public DbSet<VendorLabel> VendorLabels { get; set; }
public DbSet<InvoiceLabel> InvoiceLabels { get; set; }

public DbSet<Job> Jobs { get; set; }
public DbSet<Quotation> Quotations { get; set; }
public DbSet<Jobcard> Jobcards { get; set; }
public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

public DbSet<Inventory> Inventories { get; set; }
public DbSet<InventoryItem> InventoryItems { get; set; }
public DbSet<InventoryCategory> InventoryCategories { get; set; }
public DbSet<Quantity> Quantities { get; set; }
public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
public DbSet<TransactionType> TransactionTypes { get; set; }

public DbSet<Account> Accounts { get; set; }
public DbSet<AccountProcess> AccountProcesses { get; set; }
public DbSet<AccountPayable> AccountPayables { get; set; }
public DbSet<AccountReceivable> AccountReceivables { get; set; }

public DbSet<Payment> Payments { get; set; }
public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
public DbSet<PaymentMethod> PaymentMethods { get; set; }
public DbSet<ReceiptStatus> ReceiptStatuses { get; set; }
public DbSet<ReceiptStatusUpdate> ReceiptStatusUpdates { get; set; }

public DbSet<Vendor> Vendors { get; set; }
public DbSet<VendorData> VendorDatas { get; set; }
public DbSet<Address> Addresses { get; set; }

public DbSet<Type> Types { get; set; }
public DbSet<Status> Statuses { get; set; }
        
    }
}
