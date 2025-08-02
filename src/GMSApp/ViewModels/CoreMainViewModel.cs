using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public partial class CoreMainViewModel : ObservableObject
    {
        private readonly Repository<CoreMain> _coreMainRepository;

        public ObservableCollection<CoreMain> CoreMains { get; } = new();

        [ObservableProperty]
        private CoreMain? selectedCoreMain;

        public CoreMainViewModel(Repository<CoreMain> coreMainRepository)
        {
            _coreMainRepository = coreMainRepository;
            _ = LoadAsync();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            CoreMains.Clear();
            var items = await _coreMainRepository.GetAllAsync();
            foreach (var item in items)
                CoreMains.Add(item);
        }

        [RelayCommand]
        public async Task AddAsync()
        {
            var coreMain = new CoreMain();

            await _coreMainRepository.AddAsync(coreMain);
            await LoadAsync();
            SelectedCoreMain = coreMain;
        }
    }
}