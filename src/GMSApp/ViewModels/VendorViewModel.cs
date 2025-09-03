// VendorViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels
{
    public partial class VendorViewModel : ObservableObject
    {
        private readonly IRepository<Vendor> _repo;

        public ObservableCollection<Vendor> Vendors { get; } = new();

        public VendorViewModel(IRepository<Vendor> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private Vendor? selectedVendor;

        partial void OnSelectedVendorChanged(Vendor? value)
        {
            // Notify commands so Save/Delete enable state updates
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
                Vendors.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var v in list)
                {
                    Vendors.Add(v);
                }

                SelectedVendor = Vendors.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load vendors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create a draft vendor in-memory. Not persisted until SaveAsync is called.
        [RelayCommand]
        public Task AddAsync()
        {
            try
            {
                var v = new Vendor
                {
                    // Id = 0 means not yet persisted
                    Name = string.Empty,
                    ContactPerson = string.Empty,
                    Phone = string.Empty,
                    Email = string.Empty,
                    CRNumber = string.Empty
                };

                Vendors.Add(v);
                SelectedVendor = v;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create vendor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedVendor == null) return;

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(SelectedVendor.Name))
                {
                    MessageBox.Show("Vendor name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create a detached copy to avoid passing UI-tracked objects to repository/EF
                var detached = new Vendor
                {
                    Id = SelectedVendor.Id,
                    Name = SelectedVendor.Name?.Trim() ?? string.Empty,
                    ContactPerson = string.IsNullOrWhiteSpace(SelectedVendor.ContactPerson) ? null : SelectedVendor.ContactPerson?.Trim(),
                    Phone = string.IsNullOrWhiteSpace(SelectedVendor.Phone) ? null : SelectedVendor.Phone?.Trim(),
                    Email = string.IsNullOrWhiteSpace(SelectedVendor.Email) ? null : SelectedVendor.Email?.Trim(),
                    CRNumber = string.IsNullOrWhiteSpace(SelectedVendor.CRNumber) ? null : SelectedVendor.CRNumber?.Trim(),
                    AddressId = SelectedVendor.AddressId,
                    Address = SelectedVendor.Address
                };

                if (detached.Id == 0)
                {
                    await _repo.AddAsync(detached);
                }
                else
                {
                    await _repo.UpdateAsync(detached);
                }

                // Reload canonical list (to get assigned Ids / server defaults)
                await LoadAsync();

                // Restore selection by Id or Name
                SelectedVendor = Vendors.FirstOrDefault(x => x.Id == detached.Id) ??
                                 Vendors.FirstOrDefault(x => x.Name == detached.Name) ??
                                 SelectedVendor;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : string.Empty;
                MessageBox.Show($"Failed to save vendor: {ex.Message}{inner}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedVendor == null) return;

            var confirm = MessageBox.Show("Delete selected vendor?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedVendor.Id == 0)
                {
                    // Local (unsaved) vendor - just remove from collection
                    Vendors.Remove(SelectedVendor);
                    SelectedVendor = Vendors.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedVendor.Id);
                    await LoadAsync();
                    SelectedVendor = Vendors.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete vendor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedVendor != null;
    }
}