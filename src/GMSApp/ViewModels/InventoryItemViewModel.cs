using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Inventory
{
    public partial class InventoryItemViewModel : ObservableObject
    {
        private readonly IRepository<InventoryItem> _itemRepo;
        private readonly IRepository<StockTransaction> _txRepo;

        public ObservableCollection<InventoryItem> Items { get; } = new();

        public InventoryItemViewModel(IRepository<InventoryItem> itemRepo, IRepository<StockTransaction> txRepo)
        {
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _txRepo = txRepo ?? throw new ArgumentNullException(nameof(txRepo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private InventoryItem? selectedItem;

        partial void OnSelectedItemChanged(InventoryItem? value)
        {
            // ensure selected item exists
            if (value != null)
            {
                if (value.Transactions == null) value.Transactions = new System.Collections.Generic.List<StockTransaction>();
            }

            NotifyCommands();
            OnPropertyChanged(nameof(NeedsReorder));
        }

        [ObservableProperty]
        private int adjustmentQuantity;

        [ObservableProperty]
        private string adjustmentNotes = string.Empty;

        public bool NeedsReorder => SelectedItem != null && SelectedItem.QuantityInStock <= SelectedItem.ReorderLevel;

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AdjustStockCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Items.Clear();
                var list = await _itemRepo.GetAllAsync();
                foreach (var i in list)
                {
                    if (i.Transactions == null) i.Transactions = new System.Collections.Generic.List<StockTransaction>();
                    Items.Add(i);
                }

                SelectedItem = Items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load inventory items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var it = new InventoryItem
            {
                ItemCode = $"ITM-{DateTime.UtcNow:yyMMddHHmmss}",
                Name = string.Empty,
                Description = string.Empty,
                Category = string.Empty,
                SubCategory = string.Empty,
                QuantityInStock = 0,
                ReorderLevel = 5,
                Unit = "pcs",
                CostPrice = 0m,
                SellingPrice = 0m,
                Currency = "QAR",
                Location = string.Empty,
                LastRestocked = DateTime.UtcNow,
                IsActive = true
            };

            Items.Add(it);
            SelectedItem = it;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedItem == null) return;

            try
            {
                // detached copy to avoid EF tracking issues
                var detached = new InventoryItem
                {
                    Id = SelectedItem.Id,
                    ItemCode = SelectedItem.ItemCode?.Trim() ?? string.Empty,
                    Name = SelectedItem.Name?.Trim() ?? string.Empty,
                    Description = SelectedItem.Description,
                    Category = SelectedItem.Category,
                    SubCategory = SelectedItem.SubCategory,
                    QuantityInStock = SelectedItem.QuantityInStock,
                    ReorderLevel = SelectedItem.ReorderLevel,
                    Unit = SelectedItem.Unit,
                    CostPrice = SelectedItem.CostPrice,
                    SellingPrice = SelectedItem.SellingPrice,
                    Currency = SelectedItem.Currency,
                    VendorId = SelectedItem.VendorId,
                    Location = SelectedItem.Location,
                    LastRestocked = SelectedItem.LastRestocked,
                    IsActive = SelectedItem.IsActive
                };

                if (detached.Id == 0)
                    await _itemRepo.AddAsync(detached);
                else
                    await _itemRepo.UpdateAsync(detached);

                await LoadAsync();
                SelectedItem = Items.FirstOrDefault(x => x.ItemCode == detached.ItemCode) ?? SelectedItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            var confirm = MessageBox.Show("Delete selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedItem.Id == 0)
                {
                    Items.Remove(SelectedItem);
                    SelectedItem = Items.FirstOrDefault();
                }
                else
                {
                    await _itemRepo.DeleteAsync(SelectedItem.Id);
                    await LoadAsync();
                    SelectedItem = Items.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // manual stock adjustment (positive to add, negative to remove)
        [RelayCommand(CanExecute = nameof(CanAdjust))]
        public async Task AdjustStockAsync()
        {
            if (SelectedItem == null) return;
            if (AdjustmentQuantity == 0)
            {
                MessageBox.Show("Adjustment quantity must be non-zero.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // create transaction
                var tx = new StockTransaction
                {
                    InventoryItemId = SelectedItem.Id,
                    TransactionDate = DateTime.UtcNow,
                    TransactionType = AdjustmentQuantity > 0 ? StockTransactionType.Purchase : StockTransactionType.Adjustment,
                    Quantity = AdjustmentQuantity,
                    UnitPrice = SelectedItem.CostPrice,
                    Notes = AdjustmentNotes ?? string.Empty
                };

                await _txRepo.AddAsync(tx);

                // update item quantity
                SelectedItem.QuantityInStock += AdjustmentQuantity;
                SelectedItem.LastRestocked = DateTime.UtcNow;

                await _itemRepo.UpdateAsync(SelectedItem);

                // refresh
                await LoadAsync();
                MessageBox.Show("Stock adjusted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                // reset adjustment inputs
                AdjustmentQuantity = 0;
                AdjustmentNotes = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Adjustment failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedItem != null;
        private bool CanAdjust() => SelectedItem != null && AdjustmentQuantity != 0;
    }
}