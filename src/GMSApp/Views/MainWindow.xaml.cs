using GMSApp.ViewModels;
using GMSApp.Views;
using System.Windows;

namespace GMSApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly FilesPage _filesPage;

        public MainWindow(FilesPage filesPage)
        {
            InitializeComponent();
            _filesPage = filesPage;
        }

        private void FilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _filesPage;
        }
    }
}

