using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using GMSApp.Services;
using GMSApp.Views.Job;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
namespace GMSApp.ViewModels.Job;
public partial class JobOrderViewModel : ObservableObject
{
    private readonly IRepository<JobOrder> _jobRepo;
    private readonly IFileRepository _fileRepo;

    public ObservableCollection<JobOrder> Joborders { get; } = new();

    public JobOrderViewModel(IRepository<JobOrder> jobRepo, IFileRepository fileRepo)
    {
        _jobRepo = jobRepo;
        _fileRepo = fileRepo;
        
    }

    [ObservableProperty]
    private string jobNumber;

    [ObservableProperty]
    private  selectedJobOrder;

    [ObservableProperty]
    private DateTime date = DateTime.Now;


    [RelayCommand]
    private async void LoadJobOrders()
    {
        Joborders.Clear();
        var jobOrders = await _jobRepo.GetAllAsync();
        foreach (var jobOrder in jobOrders)
        {
            Joborders.Add(jobOrder);
        }
    }

   


    [RelayCommand]
    private async void ExportPdf()
    {
        if (SelectedJobOrder == null) return;
        var pdfBytes = await _fileRepo.ExportJobOrderToPdfAsync(SelectedJobOrder);
        
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            FileName = $"{SelectedJobOrder.JobNumber}.pdf"
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            await File.WriteAllBytesAsync(saveFileDialog.FileName, pdfBytes);
        }
    }

}


