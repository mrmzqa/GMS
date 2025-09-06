using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models.inventory;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Inventory
{
    public partial class InventoryViewModel : ObservableObject
    {
        private readonly IRepository<InventoryItem> _itemRepo;
        private readonly IRepository<StockTransaction> _txnRepo;

        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();
        public ObservableCollection<EditableStockTransaction> Transactions { get; } = new();

        // Helper collection for Unit enum binding in the view
        public ObservableCollection<Unit> Units { get; } = new(Enum.GetValues(typeof(Unit)).Cast<Unit>());

        public InventoryViewModel(IRepository<InventoryItem> itemRepo, IRepository<StockTransaction> txnRepo)
        {
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));

            _ = LoadAsync();
        }

        [ObservableProperty] private InventoryItem? selectedItem;
        partial void OnSelectedItemChanged(InventoryItem? value)
        {
            // When selection changes, reload transactions for that item (if any)
            _ = LoadTransactionsForSelectedAsync();
            LoadCommand.NotifyCanExecuteChanged();
            AddItemCommand.NotifyCanExecuteChanged();
            SaveItemCommand.NotifyCanExecuteChanged();
            DeleteItemCommand.NotifyCanExecuteChanged();
            AddTransactionCommand.NotifyCanExecuteChanged();
            DeleteTransactionCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty] private EditableStockTransaction? selectedTransaction;

        public decimal SelectedItemStock => SelectedItem?.QuantityInStock ?? 0;

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                InventoryItems.Clear();
                var items = await _itemRepo.GetAllAsync();
                foreach (var i in items) InventoryItems.Add(i);

                SelectedItem = InventoryItems.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load inventory items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTransactionsForSelectedAsync()
        {
            Transactions.Clear();
            if (SelectedItem == null) return;

            try
            {
                var allTx = await _txnRepo.GetAllAsync();
                var forItem = allTx.Where(t => t.InventoryItemId == SelectedItem.Id).OrderByDescending(t => t.TransactionDate);
                foreach (var t in forItem)
                {
                    Transactions.Add(new EditableStockTransaction(t));
                }
            }
            catch (Exception ex)
            {
                // Non-fatal; just show message
                MessageBox.Show($"Failed to load transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddItemAsync()
        {
            var it = new InventoryItem
            {
                ItemCode = $"IT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = "",
                Description = "",
                Category = "",
                SubCategory = "",
                QuantityInStock = 0,
                ReorderLevel = 5,
                Unit = Unit.Piece,
                CostPrice = 0m,
                SellingPrice = 0m,
                Currency = GMSApp.Models.account.Currency.QAR,
                Location = "",
                LastRestocked = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            InventoryItems.Add(it);
            SelectedItem = it;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyItem))]
        public async Task SaveItemAsync()
        {
            if (SelectedItem == null) return;

            try
            {
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
                    CreatedAt = SelectedItem.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = SelectedItem.IsActive
                };

                if (detached.Id == 0)
                    await _itemRepo.AddAsync(detached);
                else
                    await _itemRepo.UpdateAsync(detached);

                await LoadAsync();
                SelectedItem = InventoryItems.FirstOrDefault(i => i.ItemCode == detached.ItemCode) ?? SelectedItem;
                MessageBox.Show("Item saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifyItem))]
        public async Task DeleteItemAsync()
        {
            if (SelectedItem == null) return;

            var confirmed = MessageBox.Show("Delete selected inventory item? This will not remove related transactions.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmed != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedItem.Id == 0)
                {
                    InventoryItems.Remove(SelectedItem);
                    SelectedItem = InventoryItems.FirstOrDefault();
                    return;
                }

                await _itemRepo.DeleteAsync(SelectedItem.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModifyItem() => SelectedItem != null;

        // Add stock transaction (Purchase / JobUsage / Adjustment / Return)
        // This method updates stock immediately using repository and creates transaction record
        [RelayCommand(CanExecute = nameof(CanModifyItem))]
        public async Task AddTransactionAsync(EditableStockTransaction txn)
        {
            if (SelectedItem == null) return;
            if (txn == null) return;

            try
            {
                // Build transaction model
                var model = new StockTransaction
                {
                    InventoryItemId = SelectedItem.Id,
                    TransactionDate = txn.TransactionDate,
                    TransactionType = txn.TransactionType,
                    Quantity = txn.Quantity,
                    UnitPrice = txn.UnitPrice,
                    PurchaseOrderId = txn.PurchaseOrderId,
                    JobOrderId = txn.JobOrderId,
                    Notes = txn.Notes ?? string.Empty
                };

                await _txnRepo.AddAsync(model);

                // Update inventory stock according to transaction type
                // Purchase / Return => increase | JobUsage => decrease | Adjustment => signed delta (txn.Quantity can be negative)
                var item = SelectedItem;
                switch (txn.TransactionType)
                {
                    case StockTransactionType.Purchase:
                    case StockTransactionType.Return:
                        item.QuantityInStock += txn.Quantity;
                        item.LastRestocked = DateTime.UtcNow;
                        break;
                    case StockTransactionType.JobUsage:
                        item.QuantityInStock -= txn.Quantity;
                        break;
                    case StockTransactionType.Adjustment:
                        // For adjustments, the UI can supply negative quantities to reduce stock
                        item.QuantityInStock += txn.Quantity;
                        break;
                }

                if (item.QuantityInStock < 0) item.QuantityInStock = 0; // prevent negative
                item.UpdatedAt = DateTime.UtcNow;
                await _itemRepo.UpdateAsync(item);

                // refresh
                await LoadAsync();
                SelectedItem = InventoryItems.FirstOrDefault(i => i.Id == item.Id);
                MessageBox.Show("Transaction recorded and stock updated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanDeleteTransaction))]
        public async Task DeleteTransactionAsync(EditableStockTransaction txn)
        {
            if (txn == null) return;

            var confirm = MessageBox.Show("Delete selected transaction? This will NOT revert stock automatically.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (txn.Id == 0)
                {
                    Transactions.Remove(txn);
                    return;
                }

                await _txnRepo.DeleteAsync(txn.Id);
                await LoadTransactionsForSelectedAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteTransaction(EditableStockTransaction? txn) => txn != null;

        // Editable wrapper used by the UI for quick entry / listing
        public class EditableStockTransaction : ObservableObject
        {
            public EditableStockTransaction() { TransactionDate = DateTime.UtcNow; }

            public EditableStockTransaction(StockTransaction src)
            {
                Id = src.Id;
                InventoryItemId = src.InventoryItemId;
                TransactionDate = src.TransactionDate;
                TransactionType = src.TransactionType;
                Quantity = src.Quantity;
                UnitPrice = src.UnitPrice;
                PurchaseOrderId = src.PurchaseOrderId;
                JobOrderId = src.JobOrderId;
                Notes = src.Notes;
            }

            [ObservableProperty] private int id;
            [ObservableProperty] private int inventoryItemId;
            [ObservableProperty] private DateTime transactionDate;
            [ObservableProperty] private StockTransactionType transactionType;
            [ObservableProperty] private int quantity;
            [ObservableProperty] private decimal unitPrice;
            [ObservableProperty] private int? purchaseOrderId;
            [ObservableProperty] private int? jobOrderId;
            [ObservableProperty] private string? notes;

            public decimal LineValue => Math.Round(Quantity * UnitPrice, 2);
        }
    }
}