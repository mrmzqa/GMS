using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.inventory;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace GMSApp.ViewModels.Inventory
{
    public partial class StockTransactionViewModel : ObservableObject
    {
        private readonly IRepository<StockTransaction> _txnRepo;
        private readonly IRepository<InventoryItem> _itemRepo;

        public ObservableCollection<StockTransaction> Transactions { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public StockTransactionViewModel(IRepository<StockTransaction> txnRepo, IRepository<InventoryItem> itemRepo)
        {
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private StockTransaction? selectedTransaction;

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Transactions.Clear();
                InventoryItems.Clear();

                var txns = await _txnRepo.GetAllAsync();
                foreach (var t in txns) Transactions.Add(t);

                var items = await _itemRepo.GetAllAsync();
                foreach (var i in items) InventoryItems.Add(i);

                SelectedTransaction = Transactions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load transactions/items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
       

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedTransaction == null) return;
            var confirm = MessageBox.Show("Delete selected transaction? This will NOT revert stock automatically.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedTransaction.Id == 0)
                {
                    Transactions.Remove(SelectedTransaction);
                }
                else
                {
                    await _txnRepo.DeleteAsync(SelectedTransaction.Id);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedTransaction != null;
    }
}