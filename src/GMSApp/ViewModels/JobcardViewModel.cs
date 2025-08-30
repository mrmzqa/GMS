using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GMSApp.ViewModels
{
    public class JobcardViewModel : INotifyPropertyChanged
    {
        private readonly IRepository<Jobcard> _jobcardRepository;
        private readonly IGenericPdfGenerator<Jobcard> _pdfGenerator;
        private readonly IFileRepository _fileRepository;

        public ObservableCollection<Jobcard> Jobcards { get; set; } = new();

        private Jobcard? _selectedJobcard;
        public Jobcard? SelectedJobcard
        {
            get => _selectedJobcard;
            set
            {
                _selectedJobcard = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(JobcardImagePreview));
            }
        }

        public BitmapImage? JobcardImagePreview
        {
            get
            {
                if (SelectedJobcard?.Propertyimage == null) return null;

                using var ms = new MemoryStream(SelectedJobcard.Propertyimage);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand UploadImageCommand { get; }

        public JobcardViewModel(
            IRepository<Jobcard> jobcardRepo,
            IGenericPdfGenerator<Jobcard> pdfGen,
            IFileRepository fileRepo)
        {
            _jobcardRepository = jobcardRepo;
            _pdfGenerator = pdfGen;
            _fileRepository = fileRepo;

            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => SelectedJobcard != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedJobcard != null);
            ExportPdfCommand = new RelayCommand(async _ => await ExportPdfAsync());
            UploadImageCommand = new RelayCommand(_ => UploadImage(), _ => SelectedJobcard != null);
        }

        public async Task LoadAsync()
        {
            Jobcards.Clear();
            var items = await _jobcardRepository.GetAllAsync();
            foreach (var job in items)
                Jobcards.Add(job);
        }

        public async Task SaveAsync()
        {
            if (SelectedJobcard == null) return;

            if (SelectedJobcard.Id == 0)
                await _jobcardRepository.AddAsync(SelectedJobcard);
            else
                await _jobcardRepository.UpdateAsync(SelectedJobcard);

            await LoadAsync();
        }

        public async Task DeleteAsync()
        {
            if (SelectedJobcard == null) return;
            await _jobcardRepository.DeleteAsync(SelectedJobcard.Id);
            await LoadAsync();
        }

        public async Task ExportPdfAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Jobcards.pdf"
            };

            if (dialog.ShowDialog() == true)
                await _pdfGenerator.GeneratePdfAsync(Jobcards, dialog.FileName);
        }

        public void UploadImage()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Property Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                var fileBytes = File.ReadAllBytes(dialog.FileName);
                SelectedJobcard!.Propertyimage = fileBytes;
                OnPropertyChanged(nameof(JobcardImagePreview));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
