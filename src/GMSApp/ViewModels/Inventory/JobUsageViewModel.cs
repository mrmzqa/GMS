/*using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class JobUsageViewModel : ObservableObject
    {
        private readonly IRepository<JobOrder> _jobRepo;
        private readonly IRepository<InventoryItem> _itemRepo;
        private readonly IRepository<StockTransaction> _txnRepo;
        private readonly IRepository<JobOrderItem> _jobItemRepo; // optional if you persist job usage separately

        public ObservableCollection<JobOrder> JobOrders { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public JobUsageViewModel(IRepository<JobOrder> jobRepo,
                                 IRepository<InventoryItem> itemRepo,
                                 IRepository<StockTransaction> txnRepo,
                                 IRepository<JobOrderItem> jobItemRepo)
        {
            _jobRepo = jobRepo ?? throw new ArgumentNullException(nameof(jobRepo));
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));
            _jobItemRepo = jobItemRepo ?? throw new ArgumentNullException(nameof(jobItemRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private JobOrder? selectedJob;
        public ObservableCollection<JobOrderItemEditable> JobItems { get; } = new();

        partial void OnSelectedJobChanged(JobOrder? value)
        {
            JobItems.Clear();
            if (value != null)
            {
                // if job has existing items, map them here (not required)
            }

            LoadCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                JobOrders.Clear();
                InventoryItems.Clear();

                var jobs = await _jobRepo.GetAllAsync();
                foreach (var j in jobs) JobOrders.Add(j);

                var inv = await _itemRepo.GetAllAsync();
                foreach (var i in inv) InventoryItems.Add(i);

                SelectedJob = JobOrders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load jobs/items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UseItemsAsync()
        {
            if (SelectedJob == null) return;
            if (!JobItems.Any()) { MessageBox.Show("Add items to use for this job.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                // persist job items and create stock txns
                foreach (var ji in JobItems)
                {
                    // create job order item (persist if repo is available)
                    var jobItem = new JobOrderItem
                    {
                        JobOrderId = SelectedJob.Id,
                        InventoryItemId = ji.InventoryItemId,
                        QuantityUsed = ji.QuantityUsed,
                        UnitPrice = ji.UnitPrice
                    };

                    await _jobItemRepo.AddAsync(jobItem);

                    var txn = new StockTransaction
                    {
                        InventoryItemId = ji.InventoryItemId,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = StockTransactionType.JobUsage,
                        Quantity = ji.QuantityUsed,
                        UnitPrice = ji.UnitPrice,
                        JobOrderId = SelectedJob.Id,
                        Notes = $"Used in Job {SelectedJob.Id}"
                    };

                    await _txnRepo.AddAsync(txn);

                    // update inventory
                    var item = (await _itemRepo.GetAllAsync()).FirstOrDefault(i => i.Id == ji.InventoryItemId);
                    if (item != null)
                    {
                        item.QuantityInStock -= ji.QuantityUsed;
                        item.UpdatedAt = DateTime.UtcNow;
                        await _itemRepo.UpdateAsync(item);
                    }
                }

                await LoadAsync();
                MessageBox.Show("Items consumed and stock updated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to use items for job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddItem()
        {
            JobItems.Add(new JobOrderItemEditable { QuantityUsed = 1, UnitPrice = 0m });
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveItem(JobOrderItemEditable? it)
        {
            if (it == null) return;
            JobItems.Remove(it);
        }

        private bool CanModify() => SelectedJob != null;

        public class JobOrderItemEditable : ObservableObject
        {
            [ObservableProperty] private int inventoryItemId;
            [ObservableProperty] private int quantityUsed;
            partial void OnQuantityUsedChanged(int value) => OnPropertyChanged(nameof(Total));
            [ObservableProperty] private decimal unitPrice;
            partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(Total));
            public decimal Total => Math.Round(QuantityUsed * UnitPrice, 2);
        }
    }
}*/