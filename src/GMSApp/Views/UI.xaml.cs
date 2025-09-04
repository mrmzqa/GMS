

public class Status
{
    public int Id { get; set; }

    // e.g., "PurchaseOrder", "Invoice", "JobOrder", "Quotation", "Currency"
    public string Category { get; set; } = string.Empty;

    // e.g., "Draft", "Approved", "Paid", "USD"
    public string Name { get; set; } = string.Empty;

    // Optional: sorting
    public int Order { get; set; }

    // Optional: active/inactive toggle
    public bool IsActive { get; set; } = true;
}protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Status>().HasData(
        // PurchaseOrder
        new Status { Id = 1, Category = "PurchaseOrder", Name = "Draft", Order = 1 },
        new Status { Id = 2, Category = "PurchaseOrder", Name = "Approved", Order = 2 },
        new Status { Id = 3, Category = "PurchaseOrder", Name = "Rejected", Order = 3 },
        new Status { Id = 4, Category = "PurchaseOrder", Name = "Completed", Order = 4 },

        // Invoice
        new Status { Id = 5, Category = "Invoice", Name = "Draft", Order = 1 },
        new Status { Id = 6, Category = "Invoice", Name = "Paid", Order = 2 },
        new Status { Id = 7, Category = "Invoice", Name = "Overdue", Order = 3 },

        // Currency
        new Status { Id = 8, Category = "Currency", Name = "QAR", Order = 1 },
        new Status { Id = 9, Category = "Currency", Name = "USD", Order = 2 },
        new Status { Id = 10, Category = "Currency", Name = "EUR", Order = 3 }
    );
}public class StatusService
{
    private readonly AppDbContext _context;

    public StatusService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Status>> GetStatusesAsync(string category)
    {
        return await _context.Statuses
            .Where(s => s.Category == category && s.IsActive)
            .OrderBy(s => s.Order)
            .ToListAsync();
    }
}using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class StatusViewModel : ObservableObject
{
    private readonly StatusService _statusService;

    public StatusViewModel(StatusService statusService)
    {
        _statusService = statusService;
    }

    [ObservableProperty] private List<Status> purchaseOrderStatuses;
    [ObservableProperty] private Status selectedPurchaseOrderStatus;

    [ObservableProperty] private List<Status> invoiceStatuses;
    [ObservableProperty] private Status selectedInvoiceStatus;

    [ObservableProperty] private List<Status> currencies;
    [ObservableProperty] private Status selectedCurrency;

    public async Task LoadStatusesAsync()
    {
        PurchaseOrderStatuses = await _statusService.GetStatusesAsync("PurchaseOrder");
        InvoiceStatuses       = await _statusService.GetStatusesAsync("Invoice");
        Currencies            = await _statusService.GetStatusesAsync("Currency");
    }
}<StackPanel Margin="20" Orientation="Vertical" Spacing="12">

    <!-- Purchase Order -->
    <TextBlock Text="Purchase Order Status:"/>
    <ComboBox ItemsSource="{Binding PurchaseOrderStatuses}"
              SelectedItem="{Binding SelectedPurchaseOrderStatus, Mode=TwoWay}"
              DisplayMemberPath="Name"
              Width="200"/>

    <!-- Invoice -->
    <TextBlock Text="Invoice Status:"/>
    <ComboBox ItemsSource="{Binding InvoiceStatuses}"
              SelectedItem="{Binding SelectedInvoiceStatus, Mode=TwoWay}"
              DisplayMemberPath="Name"
              Width="200"/>

    <!-- Currency -->
    <TextBlock Text="Currency:"/>
    <ComboBox ItemsSource="{Binding Currencies}"
              SelectedItem="{Binding SelectedCurrency, Mode=TwoWay}"
              DisplayMemberPath="Name"
              Width="200"/>
</StackPanel>





