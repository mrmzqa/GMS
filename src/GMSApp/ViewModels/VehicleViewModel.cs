using GMSApp.Models;
using GMSApp.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class VehicleViewModel : INotifyPropertyChanged
{
    private readonly IRepository<Vehicle> _vehicleRepository;

    public ObservableCollection<Vehicle> Vehicles { get; } = new();

    private Vehicle _selectedVehicle = new();
    public Vehicle SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            _selectedVehicle = value;
            OnPropertyChanged();
        }
    }

    public VehicleViewModel(IRepository<Vehicle> vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task LoadVehiclesAsync()
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        Vehicles.Clear();
        foreach (var vehicle in vehicles)
            Vehicles.Add(vehicle);
    }

    public async Task AddVehicleAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedVehicle.Make)) return;
        await _vehicleRepository.AddAsync(SelectedVehicle);
        await LoadVehiclesAsync();
        SelectedVehicle = new Vehicle();
    }

    public async Task UpdateVehicleAsync()
    {
        if (SelectedVehicle.Id == 0) return;
        await _vehicleRepository.UpdateAsync(SelectedVehicle);
        await LoadVehiclesAsync();
        SelectedVehicle = new Vehicle();
    }

    public async Task DeleteVehicleAsync()
    {
        if (SelectedVehicle.Id == 0) return;
        await _vehicleRepository.DeleteAsync(SelectedVehicle.Id);
        await LoadVehiclesAsync();
        SelectedVehicle = new Vehicle();
    }

    public async Task SearchVehiclesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            await LoadVehiclesAsync();
            return;
        }

        var results = await _vehicleRepository.FindAsync(v =>
            v.Make.ToLower().Contains(searchTerm.ToLower()) ||
            v.Model.ToLower().Contains(searchTerm.ToLower()) ||
            v.LicensePlate.ToLower().Contains(searchTerm.ToLower()));

        Vehicles.Clear();
        foreach (var v in results)
            Vehicles.Add(v);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
