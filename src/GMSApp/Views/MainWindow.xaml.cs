using GMSApp.ViewModels;
using GMSApp.Views;
using System.Windows;

namespace GMSApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly FilesPage _filesPage;
        private readonly VehiclePage _vehiclePage;

        public MainWindow(FilesPage filesPage , VehiclePage vehiclePage)
        {
            InitializeComponent();
            _filesPage = filesPage;
            _vehiclePage = vehiclePage;

        }

        private void FilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _filesPage;
        }
    }
}

