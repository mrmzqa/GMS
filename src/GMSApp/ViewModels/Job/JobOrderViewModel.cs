using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Models.job;
using GMSApp.Repositories;
using GMSApp.Services;
using GMSApp.Views.Job;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
namespace GMSApp.ViewModels.Job;
public partial class JoborderViewModel : ObservableObject
{
    private readonly IRepository<Joborder> _JoborderRepo;
    private readonly IFileRepository _fileRepo;

    public ObservableCollection<Joborder> Joborders { get; } = new();

    public JoborderViewModel(IRepository<Joborder> JoborderRepo, IFileRepository fileRepo)
    {
        _JoborderRepo = JoborderRepo;
        _fileRepo = fileRepo;
        _ = LoadJobordersAsync();
    }



    [ObservableProperty]
    private Joborder? selectedJoborder;

    partial void OnSelectedJoborderChanged(Joborder? value)
    {
        UpdateJoborderCommand.NotifyCanExecuteChanged();
        DeleteJoborderCommand.NotifyCanExecuteChanged();
        FrontCommand.NotifyCanExecuteChanged();
        BackCommand.NotifyCanExecuteChanged();
        LeftSide.NotifyCanExecuteChanged();
        RightSide.NotifyCanExecuteChanged();
       
    }

    [RelayCommand]
    public async Task LoadJobordersAsync()
    {
        Joborders.Clear();
        var items = await _JoborderRepo.GetAllAsync();
        foreach (var item in items)
            Joborders.Add(item);
        SelectedJoborder = Joborders.Count > 0 ? Joborders[0] : null;
    }

    [RelayCommand]
    public async Task AddJoborderAsync()
    {
        // If SelectedJoborder is null, create a blank Joborder
        var newJoborder = SelectedJoborder != null
            ? new Joborder
            {
                CustomerName = SelectedJoborder.CustomerName,
                Phonenumber = SelectedJoborder.Phonenumber,
                VehicleNumber = SelectedJoborder.VehicleNumber,
                Brand = SelectedJoborder.Brand,
                Model = SelectedJoborder.Model,
                OdoNumber = SelectedJoborder.OdoNumber,
                F = SelectedJoborder.F,
                FN = SelectedJoborder.FN,
                B = SelectedJoborder.B,
                BN = SelectedJoborder.BN,
                LS = SelectedJoborder.LS,
                LSN = SelectedJoborder.LSN,
                RS = SelectedJoborder.RS,
                RSN = SelectedJoborder.RSN,
            }
            : new Joborder(); // Blank or with default values

        await _JoborderRepo.AddAsync(newJoborder);
        await LoadJobordersAsync();
        SelectedJoborder = newJoborder;
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task UpdateJoborderAsync()
    {
        if (SelectedJoborder == null) return;
        await _JoborderRepo.UpdateAsync(SelectedJoborder);
        await LoadJobordersAsync();
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    public async Task DeleteJoborderAsync()
    {
        if (SelectedJoborder == null) return;
        await _JoborderRepo.DeleteAsync(SelectedJoborder.Id);
        SelectedJoborder = null;
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
            Title = "Select Footer Image",
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
            Title = "Select Footer Image",
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


