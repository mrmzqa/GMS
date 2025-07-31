using GMSApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class VehiclePage : UserControl
    {
        private readonly VehicleViewModel _vm;

        public VehiclePage(VehicleViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }

        private void NewVehicle_Click(object sender, RoutedEventArgs e)
        {
            _vm.SelectedVehicle = new Models.Vehicle();
        }
    }
}