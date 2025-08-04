using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace GMSApp.ViewModels;

public partial class CoreMainViewModel : ObservableObject
{
    private readonly IRepository<CoreMain> _coreMainRepo;
    private readonly IFileRepository _fileRepo;

    public ObservableCollection<CoreMain> CoreMains { get; } = new();

    public CoreMainViewModel(IRepository<CoreMain> coreMainRepo, IFileRepository fileRepo)
    {
        _coreMainRepo = coreMainRepo;
        _fileRepo = fileRepo;
        _ = LoadCoreMainsAsync();
    }

    [ObservableProperty]
    private CoreMain? selectedCoreMain;

    partial void OnSelectedCoreMainChanged(CoreMain? value)
    {
        UpdateCoreMainCommand.NotifyCanExecuteChanged();
        DeleteCoreMainCommand.NotifyCanExecuteChanged();
        UploadHeaderFileCommand.NotifyCanExecuteChanged();
        UploadFooterFileCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task LoadCoreMainsAsync()
    {
        CoreMains.Clear();
        var items = await _coreMainRepo.GetAllAsync();
        foreach (var item in items)
            CoreMains.Add(item);
    }

    [RelayCommand]
    public async Task AddCoreMainAsync()
    {
        var newCoreMain = new CoreMain { Name = "New CoreMain" };
        await _coreMainRepo.AddAsync(newCoreMain);
        await LoadCoreMainsAsync();
        SelectedCoreMain = newCoreMain;
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task UpdateCoreMainAsync()
    {
        if (SelectedCoreMain == null) return;
        await _coreMainRepo.UpdateAsync(SelectedCoreMain);
        await LoadCoreMainsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task DeleteCoreMainAsync()
    {
        if (SelectedCoreMain == null) return;
        await _coreMainRepo.DeleteAsync(SelectedCoreMain.Id);
        SelectedCoreMain = null;
        await LoadCoreMainsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public void UploadHeaderFile()
    {
        if (SelectedCoreMain == null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Select Header Image",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedCoreMain.HeaderFile = File.ReadAllBytes(dialog.FileName);
            SelectedCoreMain.HeaderName = Path.GetFileName(dialog.FileName);
            OnPropertyChanged(nameof(SelectedCoreMain));
        }
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public void UploadFooterFile()
    {
        if (SelectedCoreMain == null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Select Footer Image",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedCoreMain.FooterFile = File.ReadAllBytes(dialog.FileName);
            SelectedCoreMain.FooterName = Path.GetFileName(dialog.FileName);
            OnPropertyChanged(nameof(SelectedCoreMain));
        }
    }

    private bool CanModify() => SelectedCoreMain != null;
}
