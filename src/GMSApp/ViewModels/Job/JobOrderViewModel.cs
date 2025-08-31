using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.job;
using GMSApp.Repositories;
using GMSApp.Views.Job;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
namespace GMSApp.ViewModels.Job
{
    public partial class JoborderViewModel : ObservableObject
    {
        private readonly IRepository<Joborder> _joborderRepo;
        private readonly IFileRepository _fileRepo;

        public ObservableCollection<Joborder> Joborders { get; } = new();

        public ObservableCollection<ItemRow> Items { get; } = new();

        public decimal Total => Items.Sum(x => x.Total);

        public JoborderViewModel(IRepository<Joborder> joborderRepo, IFileRepository fileRepo)
        {
            _joborderRepo = joborderRepo;
            _fileRepo = fileRepo;

            Items.CollectionChanged += Items_CollectionChanged;

            _ = LoadJobordersAsync();
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var ni in e.NewItems.OfType<INotifyPropertyChanged>())
                    ni.PropertyChanged += Item_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (var oi in e.OldItems.OfType<INotifyPropertyChanged>())
                    oi.PropertyChanged -= Item_PropertyChanged;
            }

            OnPropertyChanged(nameof(Total));
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemRow.Quantity) || e.PropertyName == nameof(ItemRow.Price) || e.PropertyName == nameof(ItemRow.Name))
                OnPropertyChanged(nameof(Total));
        }

        [ObservableProperty]
        private Joborder? selectedJoborder;

        partial void OnSelectedJoborderChanged(Joborder? value)
        {
            // When the SelectedJoborder changes, sync Items collection from the model
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.Clear();
            if (value?.Items != null)
            {
                foreach (var it in value.Items)
                    Items.Add(it);
            }
            Items.CollectionChanged += Items_CollectionChanged;

            // Subscribe individual item PropertyChanged to recalc totals (ItemRow implements ObservableObject)
            foreach (var item in Items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged -= Item_PropertyChanged;
            foreach (var item in Items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged += Item_PropertyChanged;

            UpdateJoborderCommand.NotifyCanExecuteChanged();
            DeleteJoborderCommand.NotifyCanExecuteChanged();
            FrontFileCommand.NotifyCanExecuteChanged();
            BackFileCommand.NotifyCanExecuteChanged();
            LeftFileCommand.NotifyCanExecuteChanged();
            RightFileCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        public async Task LoadJobordersAsync()
        {
            Joborders.Clear();
            var items = await _joborderRepo.GetAllAsync();
            foreach (var item in items)
                Joborders.Add(item);

            SelectedJoborder = Joborders.Count > 0 ? Joborders[0] : null;
        }

        [RelayCommand]
        public async Task AddJoborderAsync()
        {
            var newJoborder = new Joborder
            {
                CustomerName = SelectedJoborder?.CustomerName,
                Phonenumber = SelectedJoborder?.Phonenumber,
                VehicleNumber = SelectedJoborder?.VehicleNumber,
                Brand = SelectedJoborder?.Brand,
                Model = SelectedJoborder?.Model,
                OdoNumber = SelectedJoborder?.OdoNumber,
                Items = Items.ToList()
            };

            await _joborderRepo.AddAsync(newJoborder);
            await LoadJobordersAsync();
            SelectedJoborder = newJoborder;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            // ensure Items in the model are updated
            SelectedJoborder.Items = Items.ToList();

            await _joborderRepo.UpdateAsync(SelectedJoborder);
            await LoadJobordersAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;
            await _joborderRepo.DeleteAsync(SelectedJoborder.Id);
            SelectedJoborder = null;
            await LoadJobordersAsync();
        }

        [RelayCommand]
        private void AddItem()
        {
            var it = new ItemRow { Name = string.Empty, Quantity = 1, Price = 0m };
            Items.Add(it);
        }

        [RelayCommand]
        private void RemoveItem(ItemRow item)
        {
            if (item == null) return;
            Items.Remove(item);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveCommand()
        {
            if (SelectedJoborder == null) return;

            // Persist items into the model and update repository
            SelectedJoborder.Items = Items.ToList();
            if (SelectedJoborder.Id == 0)
            {
                await _joborderRepo.AddAsync(SelectedJoborder);
            }
            else
            {
                await _joborderRepo.UpdateAsync(SelectedJoborder);
            }

            await LoadJobordersAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void FrontFile()
        {
            if (SelectedJoborder == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Front Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

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

            var dialog = new OpenFileDialog
            {
                Title = "Select Back Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

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

            var dialog = new OpenFileDialog
            {
                Title = "Select Left Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

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

            var dialog = new OpenFileDialog
            {
                Title = "Select Right Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.RS = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.RSN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        private bool CanModify() => SelectedJoborder != null;
    }
}



