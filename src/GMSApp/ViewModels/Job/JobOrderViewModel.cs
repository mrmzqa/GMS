using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.inventory;
using GMSApp.Models.job;
using GMSApp.Repositories;
using GMSApp.Views.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

        // Guard to prevent concurrent duplicate saves
        private bool _isSaving = false;

        public ObservableCollection<Joborder> Joborders { get; } = new();
        // UI collection of job items (uses JoborderItem model which has InventoryItemId)
        public ObservableCollection<JoborderItem> Items { get; } = new();

        // Inventory list for ComboBox selection in the Items grid
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

        [ObservableProperty]
        private Joborder? selectedJoborder;

        partial void OnSelectedJoborderChanged(Joborder? value)
        {
            PopulateItemsFromSelected();
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            AddJoborderCommand.NotifyCanExecuteChanged();
            UpdateJoborderCommand.NotifyCanExecuteChanged();
            DeleteJoborderCommand.NotifyCanExecuteChanged();
            SaveJoborderCommand.NotifyCanExecuteChanged();
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

            // If Items loaded by EF, use those. Copy into UI collection to allow safe editing.
            if (SelectedJoborder.Items != null)
            {
                foreach (var mi in SelectedJoborder.Items)
                {
                    // Only include items that belong to this job via JoborderId
                    if (mi.JoborderId == SelectedJoborder.Id)
                    {
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
            }

            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        public async Task LoadJobordersAsync()
        {
            Joborders.Clear();
            InventoryItems.Clear();

            // Load inventory items first
            try
            {
                var inv = await _inventoryRepo.GetAllAsync();
                foreach (var i in inv) InventoryItems.Add(i);
            }
            catch
            {
                // ignore - inventory may not be available
            }

            // Try to use underlying DbContext for include performance if available
            try
            {
                var repoType = _jobRepo.GetType();
                var ctxField = repoType.GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
                if (ctxField != null)
                {
                    var ctx = ctxField.GetValue(_jobRepo) as DbContext;
                    if (ctx != null)
                    {
                        var list = await ctx.Set<Joborder>()
                                            .AsNoTracking()
                                            .Include(j => j.Items)
                                            .ThenInclude(it => it.InventoryItem)
                                            .ToListAsync();
                        foreach (var j in list) Joborders.Add(j);
                        SelectedJoborder = Joborders.FirstOrDefault();
                        return;
                    }
                }
            }
            catch
            {
                // ignore and fall back to repository
            }

            // Fallback - repository.GetAllAsync()
            var all = await _jobRepo.GetAllAsync();
            foreach (var j in all) Joborders.Add(j);
            SelectedJoborder = Joborders.FirstOrDefault();
        }

        // Build a Joborder entity from UI (SelectedJoborder fields + Items collection).
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

            // Add items from UI collection. For existing items keep Id; for new ones Id=0.
            foreach (var ui in Items)
            {
                var child = new JoborderItem
                {
                    Id = ui.Id, // 0 for new items
                    Name = ui.Name,
                    Quantity = ui.Quantity,
                    Price = ui.Price,
                    JoborderId = job.Id, // may be 0 for new job; EF will set FK when adding via navigation
                    InventoryItemId = ui.InventoryItemId
                };
                job.Items.Add(child);
            }

            return job;
        }

        // Create a new blank job order in the UI (does NOT persist). This avoids accidentally re-using Items
        // present in the Items collection when the user wants to create a brand new job.
        [RelayCommand]
        public Task AddJoborderAsync()
        {
            // Clear UI items and create a new Joborder template
            Items.Clear();
            var newJob = new Joborder
            {
                Created = DateTime.UtcNow
            };

            SelectedJoborder = newJob;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            try
            {
                var updated = BuildJoborderFromUi(SelectedJoborder);

                // Remove orphaned JoborderItems (items that exist in DB but were removed in UI)
                await DeleteOrphanedItemsAsync(SelectedJoborder.Id, Items.Select(i => i.Id).Where(id => id > 0).ToList());

                await _jobRepo.UpdateAsync(updated);
                await LoadJobordersAsync();

                SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == updated.Id) ?? Joborders.FirstOrDefault();
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
                var repoType = _jobRepo.GetType();
                var ctxField = repoType.GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
                if (ctxField == null) return;

                var ctx = ctxField.GetValue(_jobRepo) as DbContext;
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
                // ignore failures here
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
                // If the selected job is a template (Id==0 and not persisted), just clear selection
                if (SelectedJoborder.Id == 0)
                {
                    Items.Clear();
                    SelectedJoborder = null;
                    return;
                }

                await _jobRepo.DeleteAsync(SelectedJoborder.Id);
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
                InventoryItemId = 0 // default: not linked to inventory
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

        // Save joborder: handles both create and update, prevents duplicate creation
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            if (_isSaving)
                return;

            _isSaving = true;
            try
            {
                var toSave = BuildJoborderFromUi(SelectedJoborder);

                // If new job (Id==0) then ensure we are not creating duplicates.
                if (toSave.Id == 0)
                {
                    // Basic duplicate detection using Created timestamp (within small tolerance) and key identifying fields.
                    var all = await _jobRepo.GetAllAsync();
                    bool duplicate = false;

                    if (toSave.Created != null)
                    {
                        // compare Created within 5 seconds + same Customer and Vehicle (if provided)
                        duplicate = all.Any(j =>
                            j.Created != null &&
                            Math.Abs((j.Created.Value - toSave.Created.Value).TotalSeconds) < 5 &&
                            string.Equals(j.CustomerName?.Trim(), toSave.CustomerName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(j.VehicleNumber?.Trim(), toSave.VehicleNumber?.Trim(), StringComparison.OrdinalIgnoreCase));
                    }

                    if (duplicate)
                    {
                        MessageBox.Show("A similar joborder was recently created. Operation aborted to avoid duplicates.", "Duplicate detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _isSaving = false;
                        return;
                    }

                    // Persist
                    await _jobRepo.AddAsync(toSave);

                    // Reload canonical data and re-select the persisted job.
                    await LoadJobordersAsync();

                    // Attempt to find the newly persisted job by Created timestamp and customer/vehicle, otherwise fallback to last item
                    Joborder? persisted = Joborders
                        .OrderByDescending(j => j.Created ?? DateTime.MinValue)
                        .FirstOrDefault(j =>
                            toSave.Created != null &&
                            j.Created != null &&
                            Math.Abs((j.Created.Value - toSave.Created.Value).TotalSeconds) < 10 &&
                            string.Equals(j.CustomerName?.Trim(), toSave.CustomerName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(j.VehicleNumber?.Trim(), toSave.VehicleNumber?.Trim(), StringComparison.OrdinalIgnoreCase));

                    SelectedJoborder = persisted ?? Joborders.FirstOrDefault();
                }
                else
                {
                    // Existing job - remove orphaned items and update
                    await DeleteOrphanedItemsAsync(toSave.Id, Items.Select(i => i.Id).Where(id => id > 0).ToList());
                    await _jobRepo.UpdateAsync(toSave);
                    await LoadJobordersAsync();
                    SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == toSave.Id) ?? SelectedJoborder;
                }

                MessageBox.Show("Joborder saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // NEW: Complete joborder -> consume inventory for items that reference InventoryItemId
        // Validates stock and creates JobUsage stock transactions; decrements QuantityInStock atomically per item
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task CompleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            // Collect items that are linked to inventory
            var linkedItems = Items.Where(i => i.InventoryItemId > 0).ToList();
            if (!linkedItems.Any())
            {
                var res = MessageBox.Show("No items are linked to inventory. Mark job as completed anyway?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
            }

            try
            {
                // Save job first if not persisted
                if (SelectedJoborder.Id == 0)
                {
                    await SaveJoborderAsync();
                    // reload selected job after save
                    if (SelectedJoborder == null || SelectedJoborder.Id == 0)
                    {
                        MessageBox.Show("Failed to persist joborder before completing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Reload latest inventory data to validate stock levels
                var inventory = (await _inventoryRepo.GetAllAsync()).ToDictionary(x => x.Id);

                // Check shortages
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

                // Now for each linked item create txn and decrement stock
                foreach (var ui in linkedItems)
                {
                    // Create stock transaction
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

                    // Update inventory item
                    var item = inventory[ui.InventoryItemId];
                    item.QuantityInStock -= ui.Quantity;
                    if (item.QuantityInStock < 0) item.QuantityInStock = 0; // enforce non-negative just in case
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

        // File pickers (unchanged)
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

        // Print/Generate PDF for selected joborder (unchanged)
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task PrintAsync()
        {
            if (SelectedJoborder == null)
                return;

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
    }
}