using GMSApp.ViewModels;
using System.Windows;

namespace GMSApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly VehicleViewModel _vm;

        public MainWindow(VehicleViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.LoadVehiclesAsync();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            await _vm.AddVehicleAsync();
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            await _vm.UpdateVehicleAsync();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await _vm.DeleteVehicleAsync();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await _vm.SearchVehiclesAsync(SearchBox.Text);
        }
    }
}
