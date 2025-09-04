using GmsApp.Views;
using GMSApp.ViewModels;
using GMSApp.Views;
using GMSApp.Views.Job;
using System.Windows;
using System.Windows.Controls;

namespace GMSApp.Views
{
    

        public partial class MainWindow : Window
        {
            private readonly MainViewModel _viewModel;
        private readonly FilesPage _filesPage;
        private readonly Garage _Garage;
        private readonly PurchaseOrder _purchaseOrder;
        private readonly JobContentView _jobContentView;
        private readonly HContentView _hContentView;
        private readonly AcContentView _acContentView;
        private readonly VendorView _vendorView;
        public MainWindow(FilesPage filesPage, Garage Garage, PurchaseOrder purchaseOrderpage, JobContentView jobContentView, VendorView vendorView, HContentView hContentView, AcContentView acContentView)
        {
            
            InitializeComponent();
            _filesPage = filesPage;
            _Garage = Garage;
            _purchaseOrder = purchaseOrderpage;
            _jobContentView = jobContentView;
            _vendorView = vendorView;
            _hContentView = hContentView;
            _acContentView = acContentView;
            _viewModel = new MainViewModel();
                this.DataContext = _viewModel;
            }

            // Sync PasswordBox to ViewModel
            private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            {
                if (sender is PasswordBox pb)
                    _viewModel.Password = pb.Password;
            }

            // File > Logout
            private void FileLogout_Click(object sender, RoutedEventArgs e)
            {
                _viewModel.LogoutCommand.Execute(null);
            }

            // File > Exit
            private void FileExit_Click(object sender, RoutedEventArgs e)
            {
                Application.Current.Shutdown();
            }

        private void HelpGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("User guide coming soon...");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string aboutText = "Garage Management System v1.0\n© 2025 Created By Mohammed Rameez P P\nAll rights reserved.";
            MessageBox.Show(aboutText, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        

           
        private void AContent_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _acContentView;
        }
        private void HContent_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _hContentView;
        }

        private void MainContent_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _jobContentView;
        }

        private void GarageButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _Garage;
        }


        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _purchaseOrder;
        }
        private void FilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _filesPage;
        }
        private void Vendor_click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _vendorView;
        }
    }
    }






