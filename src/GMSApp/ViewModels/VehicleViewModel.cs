using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public partial class VehicleViewModel : ObservableObject
    {
        private readonly IRepository<Vehicle> _vehicleRepo;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        [ObservableProperty]
        private Vehicle selectedVehicle = new();

        public VehicleViewModel(IRepository<Vehicle> vehicleRepo)
        {
            _vehicleRepo = vehicleRepo;
            _ = LoadVehiclesAsync();
        }

        [RelayCommand]
        private async Task LoadVehiclesAsync()
        {
            Vehicles.Clear();
            var items = await _vehicleRepo.GetAllAsync();
            foreach (var item in items)
                Vehicles.Add(item);
        }

        [RelayCommand]
        private async Task AddVehicleAsync()
        {
            await _vehicleRepo.AddAsync(SelectedVehicle);
            await LoadVehiclesAsync();
            SelectedVehicle = new Vehicle();
        }

        [RelayCommand]
        private async Task UpdateVehicleAsync()
        {
            await _vehicleRepo.UpdateAsync(SelectedVehicle);
            await LoadVehiclesAsync();
        }

        [RelayCommand]
        private async Task DeleteVehicleAsync()
        {
            await _vehicleRepo.DeleteAsync(SelectedVehicle.Id);
            await LoadVehiclesAsync();
            SelectedVehicle = new Vehicle();
        }
    }
}
