using CommunityToolkit.Mvvm.Input;
using GMSApp.Commands;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GMSApp.ViewModels
{
    public class CoreMainViewModel : INotifyPropertyChanged
    {
        private readonly IRepository<Main> _MainRepo;
        private readonly IFileRepository _fileRepo;

        private Main? _selectedMain;

        public ObservableCollection<Main> Mains { get; set; } = new();

        public Main? SelectedMain
        {
            get => _selectedMain;
            set
            {
                _selectedMain = value;
                OnPropertyChanged();
                RaiseCommandCanExecuteChanged();
            }
        }

        public ICommand AddMainCommand { get; }
        public ICommand UpdateMainCommand { get; }
        public ICommand DeleteMainCommand { get; }
        public ICommand UploadHeaderFileCommand { get; }
        public ICommand UploadFooterFileCommand { get; }

        public CoreMainViewModel(IRepository<Main> MainRepo, IFileRepository fileRepo)
        {
            _MainRepo = MainRepo;
            _fileRepo = fileRepo;

            AddMainCommand = new RelayCommand(async () => await AddCoreMain());
            UpdateMainCommand = new RelayCommand(async () => await UpdateCoreMain(), () => SelectedMain != null);
            DeleteMainCommand = new RelayCommand(async () => await DeleteCoreMain(), () => SelectedMain != null);
            UploadHeaderFileCommand = new RelayCommand(() => UploadImage("Header"), () => SelectedMain != null);
            UploadFooterFileCommand = new RelayCommand(() => UploadImage("Footer"), () => SelectedMain != null);

            _ = LoadCoreMainsAsync();
        }

        private async Task LoadCoreMainsAsync()
        {
            var items = await _MainRepo.GetAllAsync();
            Mains.Clear();
            foreach (var item in items)
                Mains.Add(item);
        }

        private async Task AddCoreMain()
        {
            var newMain = new Main { Name = "New CoreMain" };
            await _MainRepo.AddAsync(newMain);
            Mains.Add(newMain);
            SelectedMain = newMain;
        }

        private async Task UpdateCoreMain()
        {
            if (SelectedMain != null)
                await _MainRepo.UpdateAsync(SelectedMain);
        }

        private async Task DeleteCoreMain()
        {
            if (SelectedMain == null) return;

            await _MainRepo.DeleteAsync(SelectedMain.Id);
            Mains.Remove(SelectedMain);
            SelectedMain = null;
        }

        private void UploadImage(string type)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                var fileBytes = File.ReadAllBytes(dialog.FileName);
                var fileName = Path.GetFileName(dialog.FileName);

                if (SelectedMain == null) return;

                if (type == "Header")
                {
                    SelectedMain.HeaderFile = fileBytes;
                    SelectedMain.HeaderName = fileName;
                }
                else if (type == "Footer")
                {
                    SelectedMain.FooterFile = fileBytes;
                    SelectedMain.FooterName = fileName;
                }

                OnPropertyChanged(nameof(SelectedMain));
            }
        }

        private void RaiseCommandCanExecuteChanged()
        {
            (UpdateMainCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteMainCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (UploadHeaderFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (UploadFooterFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
