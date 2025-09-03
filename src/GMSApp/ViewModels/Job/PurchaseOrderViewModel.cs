using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
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
        private readonly IRepository<Vendor> _vendorRepo;

        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; } = new();
        public ObservableCollection<Vendor> Vendors { get; } = new();

        public PurchaseOrderViewModel(IRepository<PurchaseOrder> repo,
                                      IFileRepository fileRepo,
                                      IGenericPdfGenerator<PurchaseOrder> pdfGenerator,
                                      IRepository<Vendor> vendorRepo) // <-- new
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
            _vendorRepo = vendorRepo ?? throw new ArgumentNullException(nameof(vendorRepo));

            _ = LoadAsync();
        }

        // PurchaseOrders selection
        [ObservableProperty]
        private PurchaseOrder? selectedPurchaseOrder;

        partial void OnSelectedPurchaseOrderChanged(PurchaseOrder? value)
        {
            UnsubscribeFromLineEvents(_previousLines);
            _previousLines = value?.Lines;
            SubscribeToLineEvents(value?.Lines);
            RecalculateTotals();

            // Also keep SelectedVendor in sync if PO has vendor set
            if (value != null)
            {
                // try to set SelectedVendor from Vendors collection if present
                var v = Vendors.FirstOrDefault(x => x.Id == value.VendorId || x.Name == value.Vendor.Name);
                if (v != null)
                    SelectedVendor = v;
                else
                    SelectedVendor = null;
            }
            else
            {
                SelectedVendor = null;
            }

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
            LoadVendorsCommand.NotifyCanExecuteChanged();
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

        // New: Load vendors (called when ComboBox is opened)
        [RelayCommand]
        public async Task LoadVendorsAsync(bool forceReload = false)
        {
            try
            {
                if (!forceReload && Vendors.Count > 0) return; // already loaded

                Vendors.Clear();
                var list = await _vendorRepo.GetAllAsync();
                foreach (var v in list)
                {
                    Vendors.Add(v);
                }

                // If a PO is selected and it has vendor id, set SelectedVendor
                if (SelectedPurchaseOrder != null && SelectedPurchaseOrder.VendorId.HasValue)
                {
                    var selected = Vendors.FirstOrDefault(x => x.Id == SelectedPurchaseOrder.VendorId.Value);
                    if (selected != null) SelectedVendor = selected;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load vendors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New: SelectedVendor binds to ComboBox selection
        [ObservableProperty]
        private Vendor? selectedVendor;

        partial void OnSelectedVendorChanged(Vendor? value)
        {
            if (SelectedPurchaseOrder == null) return;

            if (value == null)
            {
                SelectedPurchaseOrder.Vendor = null;
                SelectedPurchaseOrder.VendorId = null;
            }
            else
            {
                SelectedPurchaseOrder.Vendor.Name = value.Name; // adjust property if vendor has different name property
                SelectedPurchaseOrder.VendorId = value.Id;
            }

            // notify PO changed so UI/commands update
            OnPropertyChanged(nameof(SelectedPurchaseOrder));
        }

        [RelayCommand]
        public Task AddAsync()
        {
            try
            {
                var po = new PurchaseOrder
                {
                    PONumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Date = DateTime.UtcNow,
                    Lines = new ObservableCollection<PurchaseOrderLine>()
                };

                PurchaseOrders.Add(po);
                SelectedPurchaseOrder = po;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create new purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                RecalculateTotals();

                var detached = new PurchaseOrder
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
                    UpdatedBy = SelectedPurchaseOrder.UpdatedBy,
                    Lines = new ObservableCollection<PurchaseOrderLine>()
                };

                foreach (var l in SelectedPurchaseOrder.Lines)
                {
                    detached.Lines.Add(new PurchaseOrderLine
                    {
                        Description = l.Description,
                        PartNumber = l.PartNumber,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        Unit = l.Unit,
                        Notes = l.Notes,
                        QuantityDelivered = l.QuantityDelivered,
                        LineTotal = l.LineTotal
                    });
                }

                if (detached.Id == 0)
                    await _repo.AddAsync(detached);
                else
                    await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.PONumber == detached.PONumber) ?? SelectedPurchaseOrder;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : string.Empty;
                MessageBox.Show($"Failed to save purchase order: {ex.Message}{inner}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (SelectedPurchaseOrder.Id == 0)
                {
                    PurchaseOrders.Remove(SelectedPurchaseOrder);
                    SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedPurchaseOrder.Id);
                    SelectedPurchaseOrder = null;
                    await LoadAsync();
                }
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
                    UpdatedBy = SelectedPurchaseOrder.UpdatedBy,
                    Lines = new ObservableCollection<PurchaseOrderLine>()
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

                model.RecalculateTotals();

                var dlg = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = $"{model.PONumber}.pdf" };
                if (dlg.ShowDialog() != true) return;

                var path = dlg.FileName;
                await _pdfGenerator.GeneratePdfAsync(new[] { model }, path);

                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedPurchaseOrder != null;
    }

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