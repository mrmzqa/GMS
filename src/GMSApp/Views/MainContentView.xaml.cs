using GarageApp.ViewModels;
using GarageApp.Views;
using System.Windows.Controls;

namespace GarageApp.Views
{
    public partial class MainContentView : UserControl
    {
        private readonly VehicleViewModel _vehicleViewModel;

        public MainContentView(VehicleViewModel vehicleViewModel)
        {
            InitializeComponent();
            _vehicleViewModel = vehicleViewModel;
            VehiclesPageControl.DataContext = _vehicleViewModel;
        }
    }
}