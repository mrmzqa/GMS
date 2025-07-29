using System.Windows.Controls;
using GarageApp.ViewModels;

namespace GmsApp.Views
{
    public partial class VehiclesPage : UserControl
    {
        public VehiclesPage()
        {
            InitializeComponent();
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is VehicleViewModel vm)
            {
                await vm.SearchVehiclesAsync((sender as TextBox)?.Text ?? string.Empty);
            }
        }

        private async void ClearSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is VehicleViewModel vm)
            {
                if (SearchBox != null)
                    SearchBox.Text = string.Empty;

                await vm.LoadVehiclesAsync();
            }
        }
    }
}