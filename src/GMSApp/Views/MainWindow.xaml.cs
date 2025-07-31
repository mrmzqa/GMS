using GMSApp.Views;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace GMSApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Load default page
            MainFrame.Navigate(_serviceProvider.GetRequiredService<VehiclesPage>());
        }
     

        private void VehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(_serviceProvider.GetRequiredService<VehiclesPage>());
        }
    }
}
