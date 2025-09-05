using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.job;
using GMSApp.Repositories;
using GMSApp.Views.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
namespace GMSApp.ViewModels.Job
{
    public partial class JoborderViewModel : ObservableObject
    {
        private readonly IRepository<Joborder> _jobRepo;
        private readonly IFileRepository _fileRepo;
        private readonly IGenericPdfGenerator<Joborder> _pdfGenerator;

        public ObservableCollection<Joborder> Joborders { get; } = new();
        public ObservableCollection<JoborderItem> Items { get; } = new();

        public decimal Total => Items.Sum(i => i.Total);

        public JoborderViewModel(IRepository<Joborder> jobRepo,
                                 IFileRepository fileRepo,
                                 IGenericPdfGenerator<Joborder> pdfGenerator)
        {
            _jobRepo = jobRepo ?? throw new ArgumentNullException(nameof(jobRepo));
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));

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
                            JoborderId = mi.JoborderId
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

            // Try to get the underlying DbContext from the repository to use AsNoTracking + Include
            try
            {
                var repoType = _jobRepo.GetType();
                var ctxField = repoType.GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
                if (ctxField != null)
                {
                    var ctx = ctxField.GetValue(_jobRepo) as DbContext;
                    if (ctx != null)
                    {
                        var list = await ctx.Set<Joborder>().AsNoTracking().Include(j => j.Items).ToListAsync();
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
        // baseModel: existing persisted model (used to preserve Id when updating)
        private Joborder BuildJoborderFromUi(Joborder? baseModel = null)
        {
            var job = new Joborder
            {
                Id = baseModel?.Id ?? 0,
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
                Created = baseModel?.Created ?? DateTime.Now
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
                    JoborderId = job.Id // may be 0 for new job; EF will set FK when adding via navigation
                };
                // Attach child to job's Items so EF inserts/updates as part of the graph
                job.Items.Add(child);
            }

            return job;
        }

        [RelayCommand]
        public async Task AddJoborderAsync()
        {
            try
            {
                // Build job from UI. If SelectedJoborder is null, Save empty job with current Items (if any).
                var newJob = BuildJoborderFromUi(null);

                // For new job, ensure children are linked via navigation so EF can set FK automatically.
                // The child JoborderId property may be 0; EF will assign after insert.

                await _jobRepo.AddAsync(newJob);

                await LoadJobordersAsync();

                // Select the newly created job (if repository preserved children and lists)
                SelectedJoborder = Joborders.FirstOrDefault(j =>
                    j.CustomerName == newJob.CustomerName &&
                    j.Created.HasValue && newJob.Created.HasValue &&
                    j.Created.Value.ToString() == newJob.Created.Value.ToString()) // crude match; better to requery in real repo
                    ?? Joborders.LastOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            try
            {
                // Build a job preserving Id (baseModel = SelectedJoborder)
                var updated = BuildJoborderFromUi(SelectedJoborder);

                // Remove orphaned JoborderItems (items that exist in DB but were removed in UI)
                await DeleteOrphanedItemsAsync(SelectedJoborder.Id, Items.Select(i => i.Id).Where(id => id > 0).ToList());

                // Now update the job graph (will add new items, update existing ones)
                await _jobRepo.UpdateAsync(updated);

                await LoadJobordersAsync();

                // Re-select the updated job
                SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == updated.Id) ?? Joborders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Deletes item rows present in DB but not present in the provided uiItemIds list.
        // Uses repository's underlying DbContext if available via reflection; otherwise no-op (best-effort).
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
                    // Attach stub and remove to avoid extra query
                    ctx.Entry(entity).State = EntityState.Deleted;
                }

                await ctx.SaveChangesAsync();
            }
            catch
            {
                // ignore failures here; fall back to letting Update handle whatever it can.
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
                JoborderId = SelectedJoborder?.Id ?? 0
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

            try
            {
                // Act like Update: build job preserving Id and handle orphans
                var toSave = BuildJoborderFromUi(SelectedJoborder);

                await DeleteOrphanedItemsAsync(SelectedJoborder.Id, Items.Select(i => i.Id).Where(id => id > 0).ToList());

                if (toSave.Id == 0)
                    await _jobRepo.AddAsync(toSave);
                else
                    await _jobRepo.UpdateAsync(toSave);

                await LoadJobordersAsync();
                SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == toSave.Id) ?? SelectedJoborder;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // File pickers
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

        // Print/Generate PDF for selected joborder
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task PrintAsync()
        {
            if (SelectedJoborder == null)
                return;

            try
            {
                // Build a fresh Joborder model containing the current Items (UI edits)
                var model = BuildJoborderFromUi(SelectedJoborder); // your existing helper that returns Joborder with Items

                // Build file path
                var temp = Path.Combine(Path.GetTempPath(), $"joborder_{model.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                // Use the injected PDF generator (ensure DI registers JoborderPdfGenerator for Joborder)
                await _pdfGenerator.GeneratePdfAsync(new[] { model }, temp);

                // Open the generated PDF using the default system app
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



