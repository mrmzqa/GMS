using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public partial class FileViewModel : ObservableObject
    {
        private readonly IFileRepository _fileRepo;

        public ObservableCollection<FileItem> Files { get; } = new();

        [ObservableProperty]
        private FileItem? selectedFile;

        public FileViewModel(IFileRepository fileRepo)
        {
            _fileRepo = fileRepo;
            _ = LoadFilesAsync();
        }

        [RelayCommand]
        private async Task LoadFilesAsync()
        {
            Files.Clear();
            var files = await _fileRepo.GetAllFilesAsync();
            foreach (var file in files)
                Files.Add(file);
        }

        [RelayCommand]
        private async Task UploadFileAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter = "All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                await _fileRepo.UploadFileAsync(dialog.FileName);
                await LoadFilesAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteFileAsync()
        {
            if (selectedFile != null)
            {
                await _fileRepo.DeleteFileAsync(selectedFile.Id);
                await LoadFilesAsync();
            }
        }
    }
}
