// File: ViewModels/PurchaseOrderViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.Enums;
using GMSApp.Models.purchase;
using GMSApp.Repositories;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Job
{
    public partial class PurchaseOrderViewModel : ObservableObject
    {
        private readonly IRepository<PurchaseOrder> _repo;
        private readonly IFileRepository _fileRepo;
        private readonly IGenericPdfGenerator<PurchaseOrder> _pdfGenerator;

        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; } = new();
        // We won't use a separate line VM; DataGrid binds directly to SelectedPurchaseOrder.Lines

        public PurchaseOrderViewModel(IRepository<PurchaseOrder> repo,
                                      IFileRepository fileRepo,
                                      IGenericPdfGenerator<PurchaseOrder> pdfGenerator)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));

            _ = LoadAsync();
        }

        [ObservableProperty]
        private PurchaseOrder? selectedPurchaseOrder;

        partial void OnSelectedPurchaseOrderChanged(PurchaseOrder? value)
        {
            // unsubscribe previous
            UnsubscribeFromLineEvents(_previousLines);
            _previousLines = value?.Lines;
            SubscribeToLineEvents(value?.Lines);
            RecalculateTotals();
            NotifyCommands();
        }

        private ObservableCollection<PurchaseOrderLine>? _previousLines;

        private void SubscribeToLineEvents(ObservableCollection<PurchaseOrderLine>? lines)
        {
            if (lines == null) return;
            lines.CollectionChanged += Lines_CollectionChanged;
            foreach (var l in lines)
            {
                if (l is INotifyPropertyChanged inpc) inpc.PropertyChanged += Line_PropertyChanged;
            }
        }

        private void UnsubscribeFromLineEvents(ObservableCollection<PurchaseOrderLine>? lines)
        {
            if (lines == null) return;
            lines.CollectionChanged -= Lines_CollectionChanged;
            foreach (var l in lines)
            {
                if (l is INotifyPropertyChanged inpc) inpc.PropertyChanged -= Line_PropertyChanged;
            }
        }

        private void Lines_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Attach/detach property changed handlers
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
            // If UnitPrice or Quantity changed -> recompute totals
            if (e.PropertyName == nameof(PurchaseOrderLine.UnitPrice) ||
                e.PropertyName == nameof(PurchaseOrderLine.Quantity) ||
                e.PropertyName == nameof(PurchaseOrderLine.Description))
            {
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            if (SelectedPurchaseOrder == null) return;

            decimal subtotal = 0m;
            foreach (var line in SelectedPurchaseOrder.Lines)
            {
                // ensure LineTotal property updated by raising property change (model computes on the fly)
                line.LineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
                subtotal += line.LineTotal;
            }

            SelectedPurchaseOrder.SubTotal = Math.Round(subtotal, 2);
            SelectedPurchaseOrder.Total = Math.Round(SelectedPurchaseOrder.SubTotal - SelectedPurchaseOrder.Discount + SelectedPurchaseOrder.Tax, 2);

            OnPropertyChanged(nameof(SelectedPurchaseOrder));
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AddLineCommand.NotifyCanExecuteChanged();
            RemoveLineCommand.NotifyCanExecuteChanged();
            PrintCommand.NotifyCanExecuteChanged();
            UploadFileCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                PurchaseOrders.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var p in list)
                {
                    // ensure Lines not null
                    if (p.Lines == null) p.Lines = new ObservableCollection<PurchaseOrderLine>();
                    PurchaseOrders.Add(p);
                }

                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load purchase orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task AddAsync()
        {
            try
            {
                // Build a plain POCO to save (avoid saving UI ObservableCollection / tracked objects directly)
                var poToSave = new PurchaseOrder
                {
                    // Do not set Id (leave default 0) so EF will insert
                    PONumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Date = SelectedPurchaseOrder?.Date ?? DateTime.UtcNow,
                    VendorId = (SelectedPurchaseOrder?.VendorId > 0) ? SelectedPurchaseOrder.VendorId : 0, // or null if you change model
                    Notes = SelectedPurchaseOrder?.Notes,
                    Discount = SelectedPurchaseOrder?.Discount ?? 0m,
                    Tax = SelectedPurchaseOrder?.Tax ?? 0m,
                    Currency = SelectedPurchaseOrder?.Currency ?? Currency.QAR,
                    Status = SelectedPurchaseOrder?.Status ?? PurchaseOrderStatus.Draft,
                    PaymentMethod = SelectedPurchaseOrder?.PaymentMethod ?? PaymentMethod.BankTransfer,
                    BankName = SelectedPurchaseOrder?.BankName,
                    IBAN = SelectedPurchaseOrder?.IBAN,
                    ExpectedDeliveryDate = SelectedPurchaseOrder?.ExpectedDeliveryDate,
                    DeliveryLocation = SelectedPurchaseOrder?.DeliveryLocation,
                    CreatedAt = DateTime.UtcNow
                };

                // Map lines into a plain List<PurchaseOrderLine>
                if (SelectedPurchaseOrder?.Lines != null)
                {
                    foreach (var uiLine in SelectedPurchaseOrder.Lines)
                    {
                        // Create new POCO line. Do NOT copy Id if it's non-zero (unless you intentionally want updates)
                        var line = new PurchaseOrderLine
                        {
                            Description = uiLine.Description ?? string.Empty,
                            PartNumber = uiLine.PartNumber,
                            UnitPrice = uiLine.UnitPrice,
                            Quantity = uiLine.Quantity,
                            Unit = uiLine.Unit,
                            Notes = uiLine.Notes,
                            QuantityDelivered = uiLine.QuantityDelivered
                        };

                        // EF will set PurchaseOrderId when saving if you attach to PO.Add
                        poToSave.Lines.Add(line);
                    }
                }

                // Recalculate totals before saving
                poToSave.RecalculateTotals();

                // Save via repository
                await _repo.AddAsync(poToSave);

                // Reload list and select created PO
                await LoadAsync();
                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.PONumber == poToSave.PONumber);
            }
            catch (Exception ex)
            {
                // Show full exception (important for troubleshooting)
                MessageBox.Show($"Failed to add purchase order:\n{ex}\n\nInnerException:\n{ex.InnerException}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                // Build a detached plain entity for update
                var poToSave = new PurchaseOrder
                {
                    Id = SelectedPurchaseOrder.Id,
                    PONumber = SelectedPurchaseOrder.PONumber,
                    Date = SelectedPurchaseOrder.Date,
                    VendorId = SelectedPurchaseOrder.VendorId,
                    Notes = SelectedPurchaseOrder.Notes,
                    Discount = SelectedPurchaseOrder.Discount,
                    Tax = SelectedPurchaseOrder.Tax,
                    Currency = SelectedPurchaseOrder.Currency,
                    Status = SelectedPurchaseOrder.Status,
                    PaymentMethod = SelectedPurchaseOrder.PaymentMethod,
                    BankName = SelectedPurchaseOrder.BankName,
                    IBAN = SelectedPurchaseOrder.IBAN,
                    ExpectedDeliveryDate = SelectedPurchaseOrder.ExpectedDeliveryDate,
                    DeliveryLocation = SelectedPurchaseOrder.DeliveryLocation,
                    CreatedAt = SelectedPurchaseOrder.CreatedAt,
                    CreatedBy = SelectedPurchaseOrder.CreatedBy,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "CurrentUser" // replace with actual user
                };

                // Map lines; include existing Ids if you want EF to update them
                foreach (var uiLine in SelectedPurchaseOrder.Lines)
                {
                    var line = new PurchaseOrderLine
                    {
                        Id = uiLine.Id, // if Id==0 EF will insert; if >0 EF will update
                        Description = uiLine.Description ?? string.Empty,
                        PartNumber = uiLine.PartNumber,
                        UnitPrice = uiLine.UnitPrice,
                        Quantity = uiLine.Quantity,
                        Unit = uiLine.Unit,
                        Notes = uiLine.Notes,
                        QuantityDelivered = uiLine.QuantityDelivered
                    };
                    poToSave.Lines.Add(line);
                }

                poToSave.RecalculateTotals();

                // Call update
                await _repo.UpdateAsync(poToSave);

                // Reload and re-select
                await LoadAsync();
                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.Id == poToSave.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save purchase order:\n{ex}\n\nInnerException:\n{ex.InnerException}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            var res = MessageBox.Show("Delete selected purchase order?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            try
            {
                await _repo.DeleteAsync(SelectedPurchaseOrder.Id);
                SelectedPurchaseOrder = null;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddLine()
        {
            if (SelectedPurchaseOrder == null) return;

            var line = new PurchaseOrderLine
            {
                Description = string.Empty,
                Quantity = 1,
                UnitPrice = 0m
            };

            SelectedPurchaseOrder.Lines.Add(line);
            RecalculateTotals();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveLine(PurchaseOrderLine? line)
        {
            if (SelectedPurchaseOrder == null || line == null) return;

            SelectedPurchaseOrder.Lines.Remove(line);
            RecalculateTotals();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private async Task UploadFileAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            var dlg = new OpenFileDialog { Title = "Select file to upload", Filter = "All files|*.*" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                await _fileRepo.UploadFileAsync(dlg.FileName);
                MessageBox.Show("File uploaded successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"File upload failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private async Task PrintAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                // Build a detached copy to avoid EF tracking issues
                var model = new PurchaseOrder
                {
                    Id = SelectedPurchaseOrder.Id,
                    PONumber = SelectedPurchaseOrder.PONumber,
                    Date = SelectedPurchaseOrder.Date,
                    Vendor = SelectedPurchaseOrder.Vendor,
                    VendorId = SelectedPurchaseOrder.VendorId,
                    Notes = SelectedPurchaseOrder.Notes,
                    Discount = SelectedPurchaseOrder.Discount,
                    Tax = SelectedPurchaseOrder.Tax,
                    SubTotal = SelectedPurchaseOrder.SubTotal,
                    Total = SelectedPurchaseOrder.Total,
                    Currency = SelectedPurchaseOrder.Currency,
                    Status = SelectedPurchaseOrder.Status,
                    PaymentMethod = SelectedPurchaseOrder.PaymentMethod,
                    BankName = SelectedPurchaseOrder.BankName,
                    IBAN = SelectedPurchaseOrder.IBAN,
                    ExpectedDeliveryDate = SelectedPurchaseOrder.ExpectedDeliveryDate,
                    DeliveryLocation = SelectedPurchaseOrder.DeliveryLocation,
                    CreatedAt = SelectedPurchaseOrder.CreatedAt,
                    CreatedBy = SelectedPurchaseOrder.CreatedBy,
                    UpdatedAt = SelectedPurchaseOrder.UpdatedAt,
                    UpdatedBy = SelectedPurchaseOrder.UpdatedBy
                };

                foreach (var l in SelectedPurchaseOrder.Lines)
                {
                    model.Lines.Add(new PurchaseOrderLine
                    {
                        Description = l.Description,
                        PartNumber = l.PartNumber,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        Unit = l.Unit,
                        Notes = l.Notes,
                        QuantityDelivered = l.QuantityDelivered
                    });
                }

                model.RecalculateTotals(); // extension method from your model set (if present)

                var dlg = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = $"{model.PONumber}.pdf" };
                if (dlg.ShowDialog() != true) return;

                var path = dlg.FileName;
                await _pdfGenerator.GeneratePdfAsync(new[] { model }, path);

                // Try to open file
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch
                {
                    // ignore
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedPurchaseOrder != null;
    }

    // Extension helper to recalc totals if needed (keeps parity with previous extension)
    public static class PurchaseOrderExtensions
    {
        public static void RecalculateTotals(this PurchaseOrder po)
        {
            if (po == null) return;
            decimal subtotal = 0m;
            foreach (var line in po.Lines)
            {
                line.LineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
                subtotal += line.LineTotal;
            }
            po.SubTotal = Math.Round(subtotal, 2);
            po.Total = Math.Round(po.SubTotal - po.Discount + po.Tax, 2);
        }
    }
}