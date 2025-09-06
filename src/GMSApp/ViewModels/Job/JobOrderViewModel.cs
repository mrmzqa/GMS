using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.inventory;
using GMSApp.Models.job;
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

namespace GMSApp.ViewModels.Job
{
    public partial class JoborderViewModel : ObservableObject
    {
        private readonly IRepository<Joborder> _jobRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IGenericPdfGenerator<Joborder> _pdfGenerator;
        private readonly IRepository<InventoryItem> _inventoryRepo;
        private readonly IRepository<StockTransaction> _txnRepo;

        // Guard to prevent concurrent saves/completions
        private bool _isSaving = false;

        public ObservableCollection<Joborder> Joborders { get; } = new();
        public ObservableCollection<JoborderItem> Items { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public decimal Total => Items.Sum(i => i.Total);

        public JoborderViewModel(IRepository<Joborder> jobRepo,
                                 IFileRepository fileRepo,
                                 IGenericPdfGenerator<Joborder> pdfGenerator,
                                 IRepository<InventoryItem> inventoryRepo,
                                 IRepository<StockTransaction> txnRepo)
        {
            _jobRepo = jobRepo ?? throw new ArgumentNullException(nameof(jobRepo));
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
            _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));

            _ = LoadJobordersAsync();
        }

        [ObservableProperty] private Joborder? selectedJoborder;

        partial void OnSelectedJoborderChanged(Joborder? value)
        {
            PopulateItemsFromSelected();
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            AddJoborderCommand.NotifyCanExecuteChanged();
            SaveJoborderCommand.NotifyCanExecuteChanged();
            UpdateJoborderCommand.NotifyCanExecuteChanged();
            DeleteJoborderCommand.NotifyCanExecuteChanged();
            FrontFileCommand.NotifyCanExecuteChanged();
            BackFileCommand.NotifyCanExecuteChanged();
            LeftFileCommand.NotifyCanExecuteChanged();
            RightFileCommand.NotifyCanExecuteChanged();
            PrintCommand.NotifyCanExecuteChanged();
            CompleteJoborderCommand.NotifyCanExecuteChanged();
        }

        private void PopulateItemsFromSelected()
        {
            Items.Clear();

            if (SelectedJoborder == null) return;

            if (SelectedJoborder.Items != null)
            {
                foreach (var mi in SelectedJoborder.Items)
                {
                    // Ensure we include items that belong to this job
                    Items.Add(new JoborderItem
                    {
                        Id = mi.Id,
                        Name = mi.Name,
                        Quantity = mi.Quantity,
                        Price = mi.Price,
                        JoborderId = mi.JoborderId,
                        InventoryItemId = mi.InventoryItemId,
                        InventoryItem = mi.InventoryItem
                    });
                }
            }

            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        public async Task LoadJobordersAsync()
        {
            Joborders.Clear();
            InventoryItems.Clear();

            // Load inventory
            try
            {
                var inv = await _inventoryRepo.GetAllAsync();
                foreach (var i in inv) InventoryItems.Add(i);
            }
            catch
            {
                // ignore
            }

            // Try to use DbContext for efficient Include; fallback to repo
            try
            {
                var ctx = TryGetDbContext();
                if (ctx != null)
                {
                    var list = await ctx.Set<Joborder>()
                                        .AsNoTracking()
                                        .Include(j => j.Items)
                                        .ThenInclude(it => it.InventoryItem)
                                        .OrderByDescending(j => j.Created)
                                        .ToListAsync();
                    foreach (var j in list) Joborders.Add(j);
                    SelectedJoborder = Joborders.FirstOrDefault();
                    return;
                }
            }
            catch
            {
                // ignore
            }

            var all = await _jobRepo.GetAllAsync();
            foreach (var j in all) Joborders.Add(j);
            SelectedJoborder = Joborders.FirstOrDefault();
        }

        private Joborder BuildJoborderFromUi(Joborder? baseModel = null)
        {
            var job = new Joborder
            {
                Id = baseModel?.Id ?? SelectedJoborder?.Id ?? 0,
                CustomerName = SelectedJoborder?.CustomerName ?? baseModel?.CustomerName,
                Phonenumber = SelectedJoborder?.Phonenumber ?? baseModel?.Phonenumber,
                VehicleNumber = SelectedJoborder?.VehicleNumber ?? baseModel?.VehicleNumber,
                Brand = SelectedJoborder?.Brand ?? baseModel?.Brand,
                Model = SelectedJoborder?.Model ?? baseModel?.Model,
                OdoNumber = SelectedJoborder?.OdoNumber ?? baseModel?.OdoNumber,
                F = SelectedJoborder?.F ?? baseModel?.F,
                FN = SelectedJoborder?.FN ?? baseModel?.FN,
                B = SelectedJoborder?.B ?? baseModel?.B,
                BN = SelectedJoborder?.BN ?? baseModel?.BN,
                LS = SelectedJoborder?.LS ?? baseModel?.LS,
                LSN = SelectedJoborder?.LSN ?? baseModel?.LSN,
                RS = SelectedJoborder?.RS ?? baseModel?.RS,
                RSN = SelectedJoborder?.RSN ?? baseModel?.RSN,
                Created = baseModel?.Created ?? SelectedJoborder?.Created ?? DateTime.UtcNow
            };

            foreach (var ui in Items)
            {
                var child = new JoborderItem
                {
                    Id = ui.Id,
                    Name = ui.Name,
                    Quantity = ui.Quantity,
                    Price = ui.Price,
                    JoborderId = job.Id,
                    InventoryItemId = ui.InventoryItemId
                };
                job.Items.Add(child);
            }

            return job;
        }

        [RelayCommand]
        public Task AddJoborderAsync()
        {
            // Prepare a new blank job/template in UI (not persisted)
            Items.Clear();
            var newJob = new Joborder { Created = DateTime.UtcNow };
            SelectedJoborder = newJob;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateJoborderAsync()
        {
            if (SelectedJoborder == null) return;
            if (SelectedJoborder.Id == 0)
            {
                // If it's not persisted yet, Save should be used
                MessageBox.Show("This is a new job. Use Save to persist it first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var updated = BuildJoborderFromUi(SelectedJoborder);
                // Remove orphaned items
                await DeleteOrphanedItemsAsync(SelectedJoborder.Id, Items.Select(i => i.Id).Where(id => id > 0).ToList());

                var ctx = TryGetDbContext();
                if (ctx != null)
                {
                    ctx.Update(updated);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    await _jobRepo.UpdateAsync(updated);
                }

                await LoadJobordersAsync();
                SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == updated.Id) ?? SelectedJoborder;
                MessageBox.Show("Joborder updated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteOrphanedItemsAsync(int jobOrderId, System.Collections.Generic.List<int> uiItemIds)
        {
            if (jobOrderId <= 0) return;

            try
            {
                var ctx = TryGetDbContext();
                if (ctx == null) return;

                var dbSet = ctx.Set<JoborderItem>();
                var existingIds = await dbSet.Where(i => i.JoborderId == jobOrderId).Select(i => i.Id).ToListAsync();
                var toDelete = existingIds.Except(uiItemIds).ToList();
                if (!toDelete.Any()) return;

                foreach (var id in toDelete)
                {
                    var entity = new JoborderItem { Id = id };
                    ctx.Entry(entity).State = EntityState.Deleted;
                }

                await ctx.SaveChangesAsync();
            }
            catch
            {
                // ignore
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            var confirm = MessageBox.Show("Delete this joborder?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedJoborder.Id == 0)
                {
                    Items.Clear();
                    SelectedJoborder = null;
                    return;
                }

                var ctx = TryGetDbContext();
                if (ctx != null)
                {
                    var stub = new Joborder { Id = SelectedJoborder.Id };
                    ctx.Entry(stub).State = EntityState.Deleted;
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    await _jobRepo.DeleteAsync(SelectedJoborder.Id);
                }

                SelectedJoborder = null;
                await LoadJobordersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddItem()
        {
            var it = new JoborderItem
            {
                Name = string.Empty,
                Quantity = 1,
                Price = 0m,
                JoborderId = SelectedJoborder?.Id ?? 0,
                InventoryItemId = 0
            };
            Items.Add(it);
            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        private void RemoveItem(JoborderItem item)
        {
            if (item == null) return;
            Items.Remove(item);
            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveJoborderAsync()
        {
            if (SelectedJoborder == null) return;
            if (_isSaving) return;

            _isSaving = true;
            try
            {
                var toSave = BuildJoborderFromUi(SelectedJoborder);

                // Persist using DbContext if available to ensure children are handled properly
                var ctx = TryGetDbContext();
                if (ctx != null)
                {
                    ctx.Add(toSave);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    await _jobRepo.AddAsync(toSave);
                }

                // Reload and select the persisted job by Id
                await LoadJobordersAsync();
                SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == toSave.Id) ?? Joborders.FirstOrDefault();

                MessageBox.Show("Joborder saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isSaving = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task CompleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            var linkedItems = Items.Where(i => i.InventoryItemId > 0).ToList();
            if (!linkedItems.Any())
            {
                var res = MessageBox.Show("No items are linked to inventory. Mark job as completed anyway?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
            }

            try
            {
                if (SelectedJoborder.Id == 0)
                {
                    await SaveJoborderAsync();
                    if (SelectedJoborder == null || SelectedJoborder.Id == 0)
                    {
                        MessageBox.Show("Failed to persist joborder before completing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var inventory = (await _inventoryRepo.GetAllAsync()).ToDictionary(x => x.Id);

                var shortageList = linkedItems
                    .Where(i => !inventory.ContainsKey(i.InventoryItemId) || inventory[i.InventoryItemId].QuantityInStock < i.Quantity)
                    .Select(i =>
                    {
                        var available = inventory.ContainsKey(i.InventoryItemId) ? inventory[i.InventoryItemId].QuantityInStock : 0;
                        var name = inventory.ContainsKey(i.InventoryItemId) ? inventory[i.InventoryItemId].Name : $"ItemId {i.InventoryItemId}";
                        return $"{name}: required {i.Quantity}, available {available}";
                    }).ToList();

                if (shortageList.Any())
                {
                    var msg = "Insufficient stock for the following items:\n" + string.Join("\n", shortageList) + "\n\nPlease adjust quantities or receive stock before completing.";
                    MessageBox.Show(msg, "Stock shortage", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var ui in linkedItems)
                {
                    var txn = new StockTransaction
                    {
                        InventoryItemId = ui.InventoryItemId,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = StockTransactionType.JobUsage,
                        Quantity = ui.Quantity,
                        UnitPrice = ui.Price,
                        JobOrderId = SelectedJoborder.Id,
                        Notes = $"Used in Job {SelectedJoborder.Id}"
                    };

                    await _txnRepo.AddAsync(txn);

                    var item = inventory[ui.InventoryItemId];
                    item.QuantityInStock -= ui.Quantity;
                    if (item.QuantityInStock < 0) item.QuantityInStock = 0;
                    item.UpdatedAt = DateTime.UtcNow;
                    await _inventoryRepo.UpdateAsync(item);
                }

                MessageBox.Show("Job completed. Inventory updated and transactions created.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadJobordersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to complete joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void FrontFile()
        {
            if (SelectedJoborder == null) return;
            var dialog = new OpenFileDialog { Title = "Select Front Image", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.F = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.FN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void BackFile()
        {
            if (SelectedJoborder == null) return;
            var dialog = new OpenFileDialog { Title = "Select Back Image", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.B = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.BN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void LeftFile()
        {
            if (SelectedJoborder == null) return;
            var dialog = new OpenFileDialog { Title = "Select Left Image", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.LS = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.LSN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void RightFile()
        {
            if (SelectedJoborder == null) return;
            var dialog = new OpenFileDialog { Title = "Select Right Image", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.RS = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.RSN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task PrintAsync()
        {
            if (SelectedJoborder == null) return;

            try
            {
                var model = BuildJoborderFromUi(SelectedJoborder);
                var temp = Path.Combine(Path.GetTempPath(), $"joborder_{model.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                await _pdfGenerator.GeneratePdfAsync(new[] { model }, temp);
                var psi = new ProcessStartInfo(temp) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate/print PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedJoborder != null;

        // Try to get underlying DbContext from repository via reflection (returns null if not available)
        private DbContext? TryGetDbContext()
        {
            try
            {
                var repoType = _jobRepo.GetType();
                var ctxField = repoType.GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
                if (ctxField == null) return null;
                return ctxField.GetValue(_jobRepo) as DbContext;
            }
            catch
            {
                return null;
            }
        }
    }
}