
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.purchase;
using GMSApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
namespace GMSApp.ViewModels.Job;

public partial class PurchaseorderViewModel : ObservableObject
{
    private readonly IRepository<Purchaseorder> _repo;
    private readonly IFileRepository _fileRepo;
    private readonly IGenericPdfGenerator<Purchaseorder> _pdfGenerator;

    public ObservableCollection<Purchaseorder> Purchaseorders { get; } = new();
    public ObservableCollection<PurchaseorderLineViewModel> LineItems { get; } = new();

    public PurchaseorderViewModel(IRepository<Purchaseorder> repo,
                                  IFileRepository fileRepo,
                                  IGenericPdfGenerator<Purchaseorder> pdfGenerator)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
        _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
        _ = LoadAsync();
    }

    [ObservableProperty]
    private Purchaseorder? selectedPurchaseorder;

    partial void OnSelectedPurchaseorderChanged(Purchaseorder? value)
    {
        PopulateLineItemsFromSelected();
        NotifyCommands();
    }

    private void NotifyCommands()
    {
        AddCommand.NotifyCanExecuteChanged();
        UpdateCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        PrintCommand.NotifyCanExecuteChanged();
        UploadFileCommand.NotifyCanExecuteChanged();
    }

    private void PopulateLineItemsFromSelected()
    {
        LineItems.Clear();

        if (SelectedPurchaseorder == null) return;

        // Create editable viewmodels for each line to support immediate notifications and total updates.
        if (SelectedPurchaseorder.Lines != null)
        {
            foreach (var line in SelectedPurchaseorder.Lines)
            {
                var vm = new PurchaseorderLineViewModel(line);
                vm.PropertyChanged += Line_PropertyChanged;
                LineItems.Add(vm);
            }
        }

        RecalculateTotalsFromLineItems();
    }

    private void Line_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PurchaseorderLineViewModel.Quantity) ||
            e.PropertyName == nameof(PurchaseorderLineViewModel.UnitPrice) ||
            e.PropertyName == nameof(PurchaseorderLineViewModel.Description))
        {
            RecalculateTotalsFromLineItems();
        }
    }

    private void RecalculateTotalsFromLineItems()
    {
        if (SelectedPurchaseorder == null) return;

        // Update LineTotal for each line vm and compute SubTotal/Total
        decimal subtotal = 0m;
        foreach (var l in LineItems)
        {
            l.CalculateLineTotal();
            subtotal += l.LineTotal;
        }

        SelectedPurchaseorder.SubTotal = Math.Round(subtotal, 2);
        SelectedPurchaseorder.Total = Math.Round(SelectedPurchaseorder.SubTotal - SelectedPurchaseorder.Discount + SelectedPurchaseorder.Tax, 2);

        OnPropertyChanged(nameof(SelectedPurchaseorder));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Purchaseorders.Clear();
        try
        {
            var list = await _repo.GetAllAsync();
            foreach (var p in list) Purchaseorders.Add(p);

            SelectedPurchaseorder = Purchaseorders.FirstOrDefault();
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
            // Create an empty Purchaseorder and persist it, then reload.
            var po = new Purchaseorder
            {
                PONumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Date = DateTime.UtcNow
            };

            // Add any lines currently in the UI (optional) - attach current LineItems if present
            foreach (var li in LineItems)
            {
                po.Lines.Add(li.ToModel());
            }

            po.RecalculateTotals();

            await _repo.AddAsync(po);
            await LoadAsync();

            SelectedPurchaseorder = Purchaseorders.FirstOrDefault(p => p.PONumber == po.PONumber) ?? SelectedPurchaseorder;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Save (alias for update/add based on Id)
    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task SaveAsync()
    {
        if (SelectedPurchaseorder == null) return;

        try
        {
            // Map LineItems back to model objects and attach to SelectedPurchaseorder
            SelectedPurchaseorder.Lines = LineItems.Select(li => li.ToModel()).ToList();

            SelectedPurchaseorder.RecalculateTotals();

            if (SelectedPurchaseorder.Id == 0)
                await _repo.AddAsync(SelectedPurchaseorder);
            else
                await _repo.UpdateAsync(SelectedPurchaseorder);

            await LoadAsync();
            // re-select by Order number or Id
            SelectedPurchaseorder = Purchaseorders.FirstOrDefault(p => p.PONumber == SelectedPurchaseorder.PONumber) ?? SelectedPurchaseorder;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task UpdateAsync()
    {
        await SaveAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task DeleteAsync()
    {
        if (SelectedPurchaseorder == null) return;

        var res = MessageBox.Show("Delete selected purchase order?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res != MessageBoxResult.Yes) return;

        try
        {
            await _repo.DeleteAsync(SelectedPurchaseorder.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddLine()
    {
        var lineVm = new PurchaseorderLineViewModel()
        {
            Description = string.Empty,
            Quantity = 1,
            UnitPrice = 0m
        };
        lineVm.PropertyChanged += Line_PropertyChanged;
        LineItems.Add(lineVm);
        RecalculateTotalsFromLineItems();
    }

    [RelayCommand]
    private void RemoveLine(PurchaseorderLineViewModel? line)
    {
        if (line == null) return;
        line.PropertyChanged -= Line_PropertyChanged;
        LineItems.Remove(line);
        RecalculateTotalsFromLineItems();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    private async Task UploadFileAsync()
    {
        if (SelectedPurchaseorder == null) return;

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
        if (SelectedPurchaseorder == null) return;

        try
        {
            // Build a fresh model to pass to PDF generator (avoid EF-tracking issues)
            var model = new Purchaseorder
            {
                Id = SelectedPurchaseorder.Id,
                PONumber = SelectedPurchaseorder.PONumber,
                Date = SelectedPurchaseorder.Date,
                VendorId = SelectedPurchaseorder.VendorId,
                Vendor = SelectedPurchaseorder.Vendor,
                Notes = SelectedPurchaseorder.Notes,
                Discount = SelectedPurchaseorder.Discount,
                Tax = SelectedPurchaseorder.Tax,
                SubTotal = SelectedPurchaseorder.SubTotal,
                Total = SelectedPurchaseorder.Total,
                Currency = SelectedPurchaseorder.Currency,
                Status = SelectedPurchaseorder.Status,
                PaymentMethod = SelectedPurchaseorder.PaymentMethod,
                BankName = SelectedPurchaseorder.BankName,
                IBAN = SelectedPurchaseorder.IBAN,
                ExpectedDeliveryDate = SelectedPurchaseorder.ExpectedDeliveryDate,
                DeliveryLocation = SelectedPurchaseorder.DeliveryLocation,
                CreatedAt = SelectedPurchaseorder.CreatedAt,
                CreatedBy = SelectedPurchaseorder.CreatedBy,
                UpdatedAt = SelectedPurchaseorder.UpdatedAt,
                UpdatedBy = SelectedPurchaseorder.UpdatedBy
            };

            model.Lines = LineItems.Select(li => li.ToModel()).ToList();
            model.RecalculateTotals();

            // Ask for save location
            var dlg = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = $"{model.PONumber}.pdf" };
            if (dlg.ShowDialog() != true) return;
            var path = dlg.FileName;

            await _pdfGenerator.GeneratePdfAsync(new[] { model }, path);

            // Try to open
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch
            {
                // ignore if system can't open
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanModify() => SelectedPurchaseorder != null;

    #region Line viewmodel (inner class)
    // Small VM wrapper for PurchaseorderLine that supports INotifyPropertyChanged and computed LineTotal
    public class PurchaseorderLineViewModel : ObservableObject
    {
        public PurchaseorderLineViewModel() { }

        public PurchaseorderLineViewModel(Purchaseorderline model)
        {
            if (model == null) return;
            Id = model.Id;
            Description = model.Description;
            PartNumber = model.PartNumber;
            UnitPrice = model.UnitPrice;
            Quantity = model.Quantity;
            Unit = model.Unit;
            Notes = model.Notes;
            QuantityDelivered = model.QuantityDelivered;
        }

        public int Id { get; set; }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string? _partNumber;
        public string? PartNumber
        {
            get => _partNumber;
            set => SetProperty(ref _partNumber, value);
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        private string? _unit = "pc";
        public string? Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private decimal _quantityDelivered;
        public decimal QuantityDelivered
        {
            get => _quantityDelivered;
            set => SetProperty(ref _quantityDelivered, value);
        }

        // Computed
        public decimal LineTotal => Math.Round(UnitPrice * Quantity, 2);

        // Call to force recalculation and notify
        public void CalculateLineTotal()
        {
            OnPropertyChanged(nameof(LineTotal));
        }

        public Purchaseorderline ToModel()
        {
            return new Purchaseorderline
            {
                Id = this.Id,
                Description = this.Description ?? string.Empty,
                PartNumber = this.PartNumber,
                UnitPrice = this.UnitPrice,
                Quantity = this.Quantity,
                LineTotal = Math.Round(this.UnitPrice * this.Quantity, 2),
                Unit = this.Unit,
                Notes = this.Notes,
                QuantityDelivered = this.QuantityDelivered
            };
        }
    }

    #endregion
}

