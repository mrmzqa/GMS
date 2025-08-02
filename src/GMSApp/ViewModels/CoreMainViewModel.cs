using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public partial class CoreMainViewModel : ObservableObject
    {
        private readonly IRepository<CoreMain> _coreMainRepo;
        private readonly IFileRepository _fileRepo;

        public ObservableCollection<CoreMain> CoreMains { get; } = new();

        [ObservableProperty]
        public CoreMain? selectedCoreMain;

        public CoreMainViewModel(IRepository<CoreMain> coreMainRepo, IFileRepository fileRepo)
        {
            _coreMainRepo = coreMainRepo;
            _fileRepo = fileRepo;
            _ = LoadCoreMainsAsync();
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
            selectedCoreMain = newCoreMain;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateCoreMainAsync()
        {
            if (selectedCoreMain == null) return;
            await _coreMainRepo.UpdateAsync(selectedCoreMain);
            await LoadCoreMainsAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteCoreMainAsync()
        {
            if (selectedCoreMain == null) return;
            await _coreMainRepo.DeleteAsync(selectedCoreMain.Id);
            selectedCoreMain = null;
            await LoadCoreMainsAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void UploadHeaderFile()
        {
            if (selectedCoreMain == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Header Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };
            if (dialog.ShowDialog() == true)
            {
                selectedCoreMain.HeaderFile = File.ReadAllBytes(dialog.FileName);
                selectedCoreMain.HeaderName = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(selectedCoreMain));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void UploadFooterFile()
        {
            if (selectedCoreMain == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Footer Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };
            if (dialog.ShowDialog() == true)
            {
                selectedCoreMain.FooterFile = File.ReadAllBytes(dialog.FileName);
                selectedCoreMain.FooterName = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(selectedCoreMain));
            }
        }

        private bool CanModify() => selectedCoreMain != null;
    }
}