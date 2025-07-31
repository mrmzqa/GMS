using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System;

namespace GMSApp.ViewModels
{
    public partial class VehicleViewModel : ObservableObject
    {
        private readonly IRepository<Vehicle> _vehicleRepository;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();

        [ObservableProperty]
        private Vehicle? selectedVehicle;

        public VehicleViewModel(IRepository<Vehicle> vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
            _ = LoadVehiclesAsync();
        }

        [RelayCommand]
        public async Task LoadVehiclesAsync()
        {
            Vehicles.Clear();
            var vehicles = await _vehicleRepository.GetAllAsync();
            foreach (var v in vehicles)
                Vehicles.Add(v);
        }

        [RelayCommand]
        public async Task AddOrUpdateVehicleAsync()
        {
            if (SelectedVehicle == null)
                return;

            if (SelectedVehicle.Id == 0)
                await _vehicleRepository.AddAsync(SelectedVehicle);
            else
                await _vehicleRepository.UpdateAsync(SelectedVehicle);

            await LoadVehiclesAsync();
        }

        [RelayCommand]
        public async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle == null)
                return;

            await _vehicleRepository.DeleteAsync(SelectedVehicle.Id);
            SelectedVehicle = null;
            await LoadVehiclesAsync();
        }

        [RelayCommand]
        public void UploadImage()
        {
            if (SelectedVehicle == null)
                SelectedVehicle = new Vehicle();

            var dialog = new OpenFileDialog()
            {
                Title = "Select Vehicle Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedVehicle.Image = File.ReadAllBytes(dialog.FileName);
                SelectedVehicle.ImageContentType = Path.GetExtension(dialog.FileName)?.ToLowerInvariant() switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".bmp" => "image/bmp",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };
                OnPropertyChanged(nameof(SelectedVehicle));
            }
        }
    }
}