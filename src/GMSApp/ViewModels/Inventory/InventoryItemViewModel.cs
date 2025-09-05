
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace GMSApp.ViewModels.Inventory
{
    public partial class InventoryItemViewModel : ObservableObject
    {
        private readonly IRepository<InventoryItem> _itemRepo;

        public ObservableCollection<InventoryItem> Items { get; } = new();

        public InventoryItemViewModel(IRepository<InventoryItem> itemRepo)
        {
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private InventoryItem? selectedItem;

        partial void OnSelectedItemChanged(InventoryItem? value)
        {
            // ensure non-null references for binding (avoid null Address-like issues)
            if (value != null)
            {
                // nothing special here, but could ensure collections exist
            }

            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Items.Clear();
                var list = await _itemRepo.GetAllAsync();
                foreach (var it in list) Items.Add(it);
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
                ItemCode = $"IT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = string.Empty,
                Description = string.Empty,
                Category = string.Empty,
                SubCategory = string.Empty,
                Unit = "pcs",
                QuantityInStock = 0,
                ReorderLevel = 5,
                LastRestocked = DateTime.UtcNow
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
                SelectedItem = Items.FirstOrDefault(i => i.ItemCode == detached.ItemCode) ?? SelectedItem;
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedItem != null;
    }
}