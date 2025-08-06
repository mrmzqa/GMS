using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public partial class MainContentViewModel : ObservableObject
    {
        private readonly IRepository<Main> _mainRepo;
        private readonly IRepository<CoreMain> _CoreMainRepo;
        private readonly IFileRepository _fileRepo;

        public ObservableCollection<CoreMain> CoreMains { get; } = new();

        public ObservableCollection<Main> Mains { get; } = new();

        public MainContentViewModel(IRepository<Main> MainRepo, IFileRepository fileRepo)
        {
            _mainRepo = MainRepo;
            _fileRepo = fileRepo;
            _ = LoadMainsAsync();
        }

        [ObservableProperty]
        private Main? selectedMain;
        private CoreMain? selectedCore;



        [RelayCommand]
        public async Task LoadMainsAsync()
        {
            if (CoreMains.Count > 0)
                Mains.Clear();
            var items = await _mainRepo.GetAllAsync();
            foreach (var item in items)
                Mains.Add(item);
            SelectedMain = Mains.Count > 0 ? Mains[0] : null;
        }




    }

}


