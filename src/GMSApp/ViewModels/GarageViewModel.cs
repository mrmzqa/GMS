using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace GMSApp.ViewModels;

public partial class GarageViewModel : ObservableObject
{
    private readonly IRepository<Garage> _GarageRepo;
    private readonly IFileRepository _fileRepo;

    public ObservableCollection<Garage> Garages { get; } = new();

    public GarageViewModel(IRepository<Garage> GarageRepo, IFileRepository fileRepo)
    {
        _GarageRepo = GarageRepo;
        _fileRepo = fileRepo;
        _ = LoadGaragesAsync();
    }

  

    [ObservableProperty]
    private Garage? selectedGarage;

    partial void OnSelectedGarageChanged(Garage? value)
    {
        UpdateGarageCommand.NotifyCanExecuteChanged();
        DeleteGarageCommand.NotifyCanExecuteChanged();
        UploadHeaderFileCommand.NotifyCanExecuteChanged();
        UploadFooterFileCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadGaragesAsync()
    {
        Garages.Clear();
        var items = await _GarageRepo.GetAllAsync();
        foreach (var item in items)
            Garages.Add(item);
        SelectedGarage = Garages.Count > 0 ? Garages[0] : null;
    }

    [RelayCommand]
    public async Task AddGarageAsync()
    {
        // If SelectedGarage is null, create a blank Garage
        var newGarage = SelectedGarage != null
            ? new Garage
            {
                Name = SelectedGarage.Name,
                HeaderFile = SelectedGarage.HeaderFile,
                HeaderName = SelectedGarage.HeaderName,
                FooterFile = SelectedGarage.FooterFile
            }
            : new Garage(); // Blank or with default values

        await _GarageRepo.AddAsync(newGarage);
        await LoadGaragesAsync();
        SelectedGarage = newGarage;
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task UpdateGarageAsync()
    {
        if (SelectedGarage == null) return;
        await _GarageRepo.UpdateAsync(SelectedGarage);
        await LoadGaragesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task DeleteGarageAsync()
    {
        if (SelectedGarage == null) return;
        await _GarageRepo.DeleteAsync(SelectedGarage.Id);
        SelectedGarage = null;
        await LoadGaragesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public void UploadHeaderFile()
    {
        if (SelectedGarage == null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Select Header Image",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedGarage.HeaderFile = File.ReadAllBytes(dialog.FileName);
            SelectedGarage.HeaderName = Path.GetFileName(dialog.FileName);
            OnPropertyChanged(nameof(SelectedGarage));
        }
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public void UploadFooterFile()
    {
        if (SelectedGarage == null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Select Footer Image",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedGarage.FooterFile = File.ReadAllBytes(dialog.FileName);
            SelectedGarage.FooterName = Path.GetFileName(dialog.FileName);
            OnPropertyChanged(nameof(SelectedGarage));
        }
    }

    private bool CanModify() => SelectedGarage != null;
}
