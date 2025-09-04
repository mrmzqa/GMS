using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.invoice;
using GMSApp.Models.job;
using GMSApp.Models.purchase;
using GMSApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace GMSApp.ViewModels.Job
{
    public partial class InvoiceViewModel : ObservableObject
    {
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<Vendor> _vendorRepo;
        private readonly IRepository<PurchaseOrder> _poRepo;

        public ObservableCollection<Invoice> Invoices { get; } = new();
        public ObservableCollection<Vendor> Vendors { get; } = new();
        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; } = new();

        // Lines shown/edited in the UI for the selected invoice
        public ObservableCollection<Invoiceline> InvoiceLines { get; } = new();

        public InvoiceViewModel(IRepository<Invoice> invoiceRepo,
                                IRepository<Vendor> vendorRepo,
                                IRepository<PurchaseOrder> poRepo)
        {
            _invoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
            _vendorRepo = vendorRepo ?? throw new ArgumentNullException(nameof(vendorRepo));
            _poRepo = poRepo ?? throw new ArgumentNullException(nameof(poRepo));

            _ = LoadAsync();
        }

        [ObservableProperty]
        private Invoice? selectedInvoice;

        partial void OnSelectedInvoiceChanged(Invoice? value)
        {
            UnsubscribeFromLineEvents();
            InvoiceLines.Clear();

            if (value != null)
            {
                // Populate editable observable collection from model lines
                var lines = value.Lines ?? Enumerable.Empty<Invoiceline>();
                foreach (var l in lines)
                {
                    InvoiceLines.Add(new Invoiceline
                    {
                        Id = l.Id,
                        InvoiceId = l.InvoiceId,
                        Description = l.Description,
                        PartNumber = l.PartNumber,
                        Unit = l.Unit,
                        UnitPrice = l.UnitPrice,
                        Quantity = l.Quantity,
                        LineTotal = l.LineTotal,
                        Notes = l.Notes
                    });
                }

                // set SelectedVendor/PO based on ids if collections already loaded
                SelectedVendor = Vendors.FirstOrDefault(v => v.Id == value.VendorId);
                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.Id == value.PurchaseOrderId);
            }
            else
            {
                SelectedVendor = null;
                SelectedPurchaseOrder = null;
            }

            SubscribeToLineEvents();
            RecalculateTotals();
            NotifyCommands();
        }

        private void SubscribeToLineEvents()
        {
            InvoiceLines.CollectionChanged += InvoiceLines_CollectionChanged;
            foreach (var l in InvoiceLines)
                if (l is INotifyPropertyChanged inpc) inpc.PropertyChanged += Line_PropertyChanged;
        }

        private void UnsubscribeFromLineEvents()
        {
            InvoiceLines.CollectionChanged -= InvoiceLines_CollectionChanged;
            foreach (var l in InvoiceLines)
                if (l is INotifyPropertyChanged inpc) inpc.PropertyChanged -= Line_PropertyChanged;
        }

        private void InvoiceLines_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var ni in e.NewItems.OfType<INotifyPropertyChanged>())
                    ni.PropertyChanged += Line_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (var oi in e.OldItems.OfType<INotifyPropertyChanged>())
                    oi.PropertyChanged -= Line_PropertyChanged;
            }

            RecalculateTotals();
        }

        private void Line_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Invoiceline.UnitPrice) ||
                e.PropertyName == nameof(Invoiceline.Quantity) ||
                e.PropertyName == nameof(Invoiceline.Description))
            {
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            if (SelectedInvoice == null)
            {
                // still update totals based on invoice lines if editing a new invoice (SelectedInvoice might be a draft)
                return;
            }

            decimal subtotal = 0m;
            foreach (var l in InvoiceLines)
            {
                l.LineTotal = Math.Round(l.UnitPrice * l.Quantity, 2);
                subtotal += l.LineTotal;
            }

            SelectedInvoice.SubTotal = Math.Round(subtotal, 2);
            SelectedInvoice.Total = Math.Round(SelectedInvoice.SubTotal - SelectedInvoice.Discount + SelectedInvoice.Tax, 2);

            // Keep UI updated
            OnPropertyChanged(nameof(SelectedInvoice));
            OnPropertyChanged(nameof(Balance));
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AddLineCommand.NotifyCanExecuteChanged();
            RemoveLineCommand.NotifyCanExecuteChanged();
            LoadVendorsCommand.NotifyCanExecuteChanged();
            LoadPurchaseOrdersCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private Vendor? selectedVendor;

        partial void OnSelectedVendorChanged(Vendor? value)
        {
            if (SelectedInvoice == null) return;
            SelectedInvoice.VendorId = value?.Id;
            SelectedInvoice.Vendor = value;
            OnPropertyChanged(nameof(SelectedInvoice));
        }

        [ObservableProperty]
        private PurchaseOrder? selectedPurchaseOrder;

        partial void OnSelectedPurchaseOrderChanged(PurchaseOrder? value)
        {
            if (SelectedInvoice == null) return;
            SelectedInvoice.PurchaseOrderId = value?.Id;
            SelectedInvoice.PurchaseOrder = value;
            OnPropertyChanged(nameof(SelectedInvoice));
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Invoices.Clear();
                var list = await _invoiceRepo.GetAllAsync();
                foreach (var inv in list)
                {
                    // ensure collections are not null
                    if (inv.Lines == null) inv.Lines = new List<Invoiceline>();
                    Invoices.Add(inv);
                }

                SelectedInvoice = Invoices.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load invoices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task LoadVendorsAsync(bool forceReload = false)
        {
            try
            {
                if (!forceReload && Vendors.Count > 0) return;
                Vendors.Clear();
                var list = await _vendorRepo.GetAllAsync();
                foreach (var v in list) Vendors.Add(v);

                if (SelectedInvoice != null)
                    SelectedVendor = Vendors.FirstOrDefault(v => v.Id == SelectedInvoice.VendorId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load vendors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task LoadPurchaseOrdersAsync(bool forceReload = false)
        {
            try
            {
                if (!forceReload && PurchaseOrders.Count > 0) return;
                PurchaseOrders.Clear();
                var list = await _poRepo.GetAllAsync();
                foreach (var p in list) PurchaseOrders.Add(p);

                if (SelectedInvoice != null)
                    SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.Id == SelectedInvoice.PurchaseOrderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load purchase orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            try
            {
                var inv = new Invoice
                {
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    InvoiceDate = DateTime.UtcNow,
                    SubTotal = 0m,
                    Discount = 0m,
                    Tax = 0m,
                    AmountPaid = 0m,
                    Lines = new List<Invoiceline>()
                };

                Invoices.Add(inv);
                SelectedInvoice = inv;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                // Recalculate totals into SelectedInvoice based on InvoiceLines
                decimal subtotal = 0m;
                foreach (var l in InvoiceLines)
                {
                    l.LineTotal = Math.Round(l.UnitPrice * l.Quantity, 2);
                    subtotal += l.LineTotal;
                }

                SelectedInvoice.SubTotal = Math.Round(subtotal, 2);
                SelectedInvoice.Total = Math.Round(SelectedInvoice.SubTotal - SelectedInvoice.Discount + SelectedInvoice.Tax, 2);

                // Build detached copy to avoid EF tracking / UI-proxy issues
                var detached = new Invoice
                {
                    Id = SelectedInvoice.Id,
                    InvoiceNumber = SelectedInvoice.InvoiceNumber,
                    InvoiceDate = SelectedInvoice.InvoiceDate,
                    VendorId = SelectedInvoice.VendorId,
                    PurchaseOrderId = SelectedInvoice.PurchaseOrderId,
                    SubTotal = SelectedInvoice.SubTotal,
                    Discount = SelectedInvoice.Discount,
                    Tax = SelectedInvoice.Tax,
                    Total = SelectedInvoice.Total,
                    AmountPaid = SelectedInvoice.AmountPaid,
                    Status = SelectedInvoice.Status,
                    CreatedAt = SelectedInvoice.CreatedAt,
                    CreatedBy = SelectedInvoice.CreatedBy,
                    UpdatedAt = SelectedInvoice.UpdatedAt,
                    UpdatedBy = SelectedInvoice.UpdatedBy,
                    Lines = InvoiceLines.Select(l => new Invoiceline
                    {
                        Id = l.Id,
                        Description = l.Description,
                        PartNumber = l.PartNumber,
                        Unit = l.Unit,
                        UnitPrice = l.UnitPrice,
                        Quantity = l.Quantity,
                        LineTotal = l.LineTotal,
                        Notes = l.Notes
                    }).ToList()
                };

                // Use repository
                if (detached.Id == 0)
                    await _invoiceRepo.AddAsync(detached);
                else
                    await _invoiceRepo.UpdateAsync(detached);

                // Reload canonical data and restore selection
                await LoadAsync();
                SelectedInvoice = Invoices.FirstOrDefault(i => i.InvoiceNumber == detached.InvoiceNumber) ?? SelectedInvoice;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : string.Empty;
                MessageBox.Show($"Failed to save invoice: {ex.Message}{inner}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedInvoice == null) return;

            var confirm = MessageBox.Show("Delete selected invoice?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedInvoice.Id == 0)
                {
                    Invoices.Remove(SelectedInvoice);
                    SelectedInvoice = Invoices.FirstOrDefault();
                }
                else
                {
                    await _invoiceRepo.DeleteAsync(SelectedInvoice.Id);
                    SelectedInvoice = null;
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddLine()
        {
            if (SelectedInvoice == null) return;

            var line = new Invoiceline
            {
                Description = string.Empty,
                Quantity = 1,
                UnitPrice = 0m,
                Unit = "pc"
            };

            InvoiceLines.Add(line);
            RecalculateTotals();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveLine(Invoiceline? line)
        {
            if (SelectedInvoice == null || line == null) return;
            InvoiceLines.Remove(line);
            RecalculateTotals();
        }

        private bool CanModify() => SelectedInvoice != null;

        // Helpful bindings
        public decimal Balance => SelectedInvoice?.Balance ?? 0m;
    }
}
