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

        // Items shown/edited in the UI for the currently selected joborder.
        public ObservableCollection<ItemRow> Items { get; } = new();

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
            TryPopulateItemsFromSelected();
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            UpdateJoborderCommand.NotifyCanExecuteChanged();
            DeleteJoborderCommand.NotifyCanExecuteChanged();
            FrontFileCommand.NotifyCanExecuteChanged();
            BackFileCommand.NotifyCanExecuteChanged();
            LeftFileCommand.NotifyCanExecuteChanged();
            RightFileCommand.NotifyCanExecuteChanged();
            PrintCommand.NotifyCanExecuteChanged();
        }

        private void TryPopulateItemsFromSelected()
        {
            Items.Clear();

            if (SelectedJoborder == null) return;

            // If Items navigation is present, prefer it. Otherwise nothing to populate.
            if (SelectedJoborder.Items != null)
            {
                // Create UI editable copies (shallow) of the model items to avoid EF tracking and to allow independent editing.
                foreach (var it in SelectedJoborder.Items.Where(i => i.JoborderGuid == SelectedJoborder.OrderGuid))
                {
                    Items.Add(new ItemRow
                    {
                        Id = it.Id,
                        Name = it.Name,
                        Quantity = it.Quantity,
                        Price = it.Price,
                        JoborderGuid = it.JoborderGuid
                    });
                }
            }

            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        public async Task LoadJobordersAsync()
        {
            Joborders.Clear();

            // Try to use the underlying DbContext with AsNoTracking + Include to avoid EF tracking issues.
            try
            {
                var repoType = _jobRepo.GetType();
                var contextField = repoType.GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic);
                if (contextField != null)
                {
                    var ctx = contextField.GetValue(_jobRepo) as DbContext;
                    if (ctx != null)
                    {
                        var list = await ctx.Set<Joborder>()
                                            .AsNoTracking()
                                            .Include(j => j.Items)
                                            .ToListAsync();

                        foreach (var j in list) Joborders.Add(j);
                        SelectedJoborder = Joborders.FirstOrDefault();
                        return;
                    }
                }
            }
            catch
            {
                // ignore reflection/access problems and fallback to repository
            }

            // Fallback when DbContext not available via repo: rely on repository
            var all = await _jobRepo.GetAllAsync();
            foreach (var j in all) Joborders.Add(j);
            SelectedJoborder = Joborders.FirstOrDefault();
        }

        // Helper: build a Joborder entity from UI state (SelectedJoborder + Items)
        private Joborder BuildJoborderFromUi(Joborder? baseModel = null)
        {
            var job = new Joborder
            {
                // If baseModel provided, carry its Id and OrderGuid to preserve updates; otherwise create new GUID.
                Id = baseModel?.Id ?? 0,
                OrderGuid = baseModel?.OrderGuid ?? Guid.NewGuid(),
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

            // Build child items from UI Items collection. Use same Id for existing items, new items keep Id=0.
            foreach (var ui in Items)
            {
                var child = new ItemRow
                {
                    Id = ui.Id,
                    Name = ui.Name,
                    Quantity = ui.Quantity,
                    Price = ui.Price,
                    JoborderGuid = job.OrderGuid
                };

                job.Items.Add(child);
            }

            return job;
        }

        [RelayCommand]
        public async Task AddJoborderAsync()
        {
            try
            {
                // Build job from UI state (if user typed fields) or create empty job if none selected.
                var modelToSave = BuildJoborderFromUi(null);

                await _jobRepo.AddAsync(modelToSave);

                await LoadJobordersAsync();

                // Select the created job by OrderGuid if returned in repository listing
                SelectedJoborder = Joborders.FirstOrDefault(j => j.OrderGuid == modelToSave.OrderGuid)
                                    ?? Joborders.FirstOrDefault();
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
                // Build an updated Joborder instance (preserve Id & OrderGuid) and save.
                var toUpdate = BuildJoborderFromUi(SelectedJoborder);
                await _jobRepo.UpdateAsync(toUpdate);

                await LoadJobordersAsync();

                // Re-select the updated job by OrderGuid
                SelectedJoborder = Joborders.FirstOrDefault(j => j.OrderGuid == toUpdate.OrderGuid)
                                    ?? Joborders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            var confirm = MessageBox.Show("Are you sure you want to delete this job order?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var it = new ItemRow
            {
                Name = string.Empty,
                Quantity = 1,
                Price = 0m,
                JoborderGuid = SelectedJoborder?.OrderGuid ?? Guid.Empty
            };

            Items.Add(it);
            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        private void RemoveItem(ItemRow item)
        {
            if (item == null) return;
            Items.Remove(item);
            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveCommand()
        {
            if (SelectedJoborder == null) return;

            try
            {
                // For Save we behave like Update: build model preserving Id/OrderGuid
                var toSave = BuildJoborderFromUi(SelectedJoborder);

                if (toSave.Id == 0)
                    await _jobRepo.AddAsync(toSave);
                else
                    await _jobRepo.UpdateAsync(toSave);

                await LoadJobordersAsync();

                SelectedJoborder = Joborders.FirstOrDefault(j => j.OrderGuid == toSave.OrderGuid)
                                    ?? Joborders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save joborder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // File pickers (Front/Back/Left/Right)
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

        // Print / Generate PDF for selected Joborder
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task PrintAsync()
        {
            if (SelectedJoborder == null) return;

            try
            {
                // Build a fresh model to pass to PDF generator (ensure the items are current).
                var model = BuildJoborderFromUi(SelectedJoborder);

                var temp = Path.Combine(Path.GetTempPath(), $"joborder_{model.OrderGuid:N}.pdf");

                // Ask PDF generator to create PDF for the single joborder.
                await _pdfGenerator.GeneratePdfAsync(new[] { model }, temp);

                // Open the PDF with default app
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



