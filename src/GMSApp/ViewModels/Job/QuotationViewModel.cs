using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Commands;
using GMSApp.Models;
using GMSApp.Models.quotation;
using GMSApp.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace GMSApp.ViewModels.Job;

public partial class QuotationViewModel : ObservableObject
{
    private readonly IRepository<Quotation> _repo;

    public ObservableCollection<Quotation> Quotations { get; } = new();

    // Editable items used by the UI (INotifyPropertyChanged)
    public ObservableCollection<EditableQuotationItem> Items { get; } = new();

    public QuotationViewModel(IRepository<Quotation> repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _ = LoadAsync();
    }

    [ObservableProperty]
    private Quotation? selectedQuotation;

    partial void OnSelectedQuotationChanged(Quotation? value)
    {
        // Populate editable Items collection when selection changes
        Items.Clear();
        if (value != null)
        {
            foreach (var i in value.Items ?? Enumerable.Empty<QuotationItem>())
            {
                var ed = new EditableQuotationItem(i);
                ed.PropertyChanged += Item_PropertyChanged;
                Items.Add(ed);
            }
        }

        // Ensure totals update
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(VatAmount));
        OnPropertyChanged(nameof(GrandTotal));
        NotifyCommands();
    }

    [ObservableProperty]
    private EditableQuotationItem? selectedItem;

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditableQuotationItem.Total) ||
            e.PropertyName == nameof(EditableQuotationItem.Quantity) ||
            e.PropertyName == nameof(EditableQuotationItem.UnitPrice))
        {
            // update totals bindings
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(VatAmount));
            OnPropertyChanged(nameof(GrandTotal));
        }
    }

    private void NotifyCommands()
    {
        LoadCommand.NotifyCanExecuteChanged();
        AddCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            Quotations.Clear();
            var list = await _repo.GetAllAsync();
            foreach (var q in list)
            {
                // Ensure Items list not null
                if (q.Items == null) q.Items = new System.Collections.Generic.List<QuotationItem>();
                Quotations.Add(q);
            }

            SelectedQuotation = Quotations.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load quotations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Create a draft in-memory (not persisted until Save)
    [RelayCommand]
    public Task AddAsync()
    {
        try
        {
            var q = new Quotation
            {
                QuotationNumber = $"QT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                DateIssued = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                Items = new System.Collections.Generic.List<QuotationItem>(),
                VatRate = 0.05m
            };

            Quotations.Add(q);
            SelectedQuotation = q;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create quotation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task SaveAsync()
    {
        if (SelectedQuotation == null) return;

        try
        {
            // Build detached copy to avoid UI-tracked objects being sent to repo/EF
            var detached = new Quotation
            {
                Id = SelectedQuotation.Id,
                QuotationNumber = SelectedQuotation.QuotationNumber,
                DateIssued = SelectedQuotation.DateIssued,
                ValidUntil = SelectedQuotation.ValidUntil,
                CustomerName = SelectedQuotation.CustomerName,
                ContactNumber = SelectedQuotation.ContactNumber,
                VehicleMake = SelectedQuotation.VehicleMake,
                VehicleModel = SelectedQuotation.VehicleModel,
                VehicleYear = SelectedQuotation.VehicleYear,
                RegistrationNumber = SelectedQuotation.RegistrationNumber,
                VIN = SelectedQuotation.VIN,
                LabourCharges = SelectedQuotation.LabourCharges,
                Discount = SelectedQuotation.Discount,
                VatRate = SelectedQuotation.VatRate,
                Status = SelectedQuotation.Status,
                // map items from editable collection back to model list
                Items = Items.Select(i => new QuotationItem
                {
                    Id = i.Id,
                    RepairQuotationId = i.RepairQuotationId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            if (detached.Id == 0)
                await _repo.AddAsync(detached);
            else
                await _repo.UpdateAsync(detached);

            // reload to get canonical state and IDs
            await LoadAsync();

            // restore selection by QuotationNumber (or Id)
            SelectedQuotation = Quotations.FirstOrDefault(q => q.QuotationNumber == detached.QuotationNumber) ?? SelectedQuotation;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : string.Empty;
            MessageBox.Show($"Failed to save quotation: {ex.Message}{inner}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task DeleteAsync()
    {
        if (SelectedQuotation == null) return;

        var confirm = MessageBox.Show("Delete selected quotation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            if (SelectedQuotation.Id == 0)
            {
                // local unsaved
                Quotations.Remove(SelectedQuotation);
                SelectedQuotation = Quotations.FirstOrDefault();
            }
            else
            {
                await _repo.DeleteAsync(SelectedQuotation.Id);
                SelectedQuotation = null;
                await LoadAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete quotation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    private void AddItem()
    {
        var item = new EditableQuotationItem
        {
            Description = string.Empty,
            Quantity = 1,
            UnitPrice = 0m
        };
        item.PropertyChanged += Item_PropertyChanged;
        Items.Add(item);
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(VatAmount));
        OnPropertyChanged(nameof(GrandTotal));
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    private void RemoveItem()
    {
        if (SelectedItem == null) return;
        SelectedItem.PropertyChanged -= Item_PropertyChanged;
        Items.Remove(SelectedItem);
        SelectedItem = null;
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(VatAmount));
        OnPropertyChanged(nameof(GrandTotal));
    }

    private bool CanModify() => SelectedQuotation != null;

    // Computed totals (bound from view)
    public decimal SubTotal
    {
        get
        {
            decimal itemsTotal = Items.Sum(i => i.Total);
            return itemsTotal + (SelectedQuotation?.LabourCharges ?? 0m);
        }
    }

    public decimal VatAmount
    {
        get
        {
            var vatRate = SelectedQuotation?.VatRate ?? 0m;
            var baseAmount = SubTotal - (SelectedQuotation?.Discount ?? 0m);
            return Math.Round(baseAmount * vatRate, 2);
        }
    }

    public decimal GrandTotal
    {
        get
        {
            var grand = (SubTotal - (SelectedQuotation?.Discount ?? 0m)) + VatAmount;
            return Math.Round(grand, 2);
        }
    }

    // Editable item class used for binding to DataGrid
    public partial class EditableQuotationItem : ObservableObject
    {
        public EditableQuotationItem() { }

        public EditableQuotationItem(QuotationItem src)
        {
            Id = src.Id;
            RepairQuotationId = src.RepairQuotationId;
            Description = src.Description;
            Quantity = src.Quantity;
            UnitPrice = src.UnitPrice;
        }

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private int repairQuotationId;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private int quantity;

        partial void OnQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(Total));
        }

        [ObservableProperty]
        private decimal unitPrice;

        partial void OnUnitPriceChanged(decimal value)
        {
            OnPropertyChanged(nameof(Total));
        }

        public decimal Total => Math.Round(Quantity * UnitPrice, 2);
    }
}