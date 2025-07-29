using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public partial class VehicleViewModel : BaseViewModel
    {
        private readonly GarageDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<Vehicle> vehicles = new();

        [ObservableProperty]
        private Vehicle? selectedVehicle;

        public VehicleViewModel(GarageDbContext dbContext)
        {
            _dbContext = dbContext;
            Task.Run(async () => await LoadVehiclesAsync());
        }

        [RelayCommand]
        public async Task LoadVehiclesAsync()
        {
            var all = await _dbContext.Vehicles.ToListAsync();
            Vehicles = new ObservableCollection<Vehicle>(all);
        }

        [ICommand]
        public async Task AddVehicleAsync()
        {
            var newVehicle = new Vehicle
            {
                OwnerName = "New Owner",
                LicensePlate = "NEW000",
                Brand = "Brand",
                Model = "Model",
                Year = System.DateTime.Now.Year
            };

            _dbContext.Vehicles.Add(newVehicle);
            await _dbContext.SaveChangesAsync();

            Vehicles.Add(newVehicle);
            SelectedVehicle = newVehicle;
        }

        [ICommand(CanExecute = nameof(CanEditOrDelete))]
        public async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle != null)
            {
                _dbContext.Vehicles.Remove(SelectedVehicle);
                await _dbContext.SaveChangesAsync();
                Vehicles.Remove(SelectedVehicle);
                SelectedVehicle = null;
            }
        }

        private bool CanEditOrDelete() => SelectedVehicle != null;

        [ICommand(CanExecute = nameof(CanEditOrDelete))]
        public async Task UpdateVehicleAsync()
        {
            if (SelectedVehicle != null)
            {
                _dbContext.Vehicles.Update(SelectedVehicle);
                await _dbContext.SaveChangesAsync();
            }
        }

        [ICommand]
        public async Task SearchVehiclesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadVehiclesAsync();
                return;
            }

            var q = query.Trim().ToLower();

            var filtered = await _dbContext.Vehicles
                .Where(v => v.LicensePlate.ToLower().Contains(q)
                         || v.VIN.ToLower().Contains(q)
                         || v.OwnerName.ToLower().Contains(q))
                .ToListAsync();

            Vehicles = new ObservableCollection<Vehicle>(filtered);
        }
    }
}