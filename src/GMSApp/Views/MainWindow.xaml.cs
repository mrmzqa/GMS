using GmsApp.Views;
using GMSApp.ViewModels;
using GMSApp.Views;
using GMSApp.Views.Job;
using System.Windows;

namespace GMSApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly FilesPage _filesPage;
        private readonly CoreMain _coreMain;
        private readonly PurchaseOrderPage _purchaseOrderPage;
        private readonly MainContentView _mainContentView;


        public MainWindow(FilesPage filesPage, CoreMain coreMain,PurchaseOrderPage purchaseOrderpage, MainContentView mainContentView)
        {
            InitializeComponent();
            _filesPage = filesPage;
            _coreMain = coreMain;
            _purchaseOrderPage = purchaseOrderpage;
            _mainContentView = mainContentView;
        }
        
        private void MainContent_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _mainContentView;
        }

        private void CoreMainButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _coreMain;
        }


        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _purchaseOrderPage;
        }
        private void FilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _filesPage;
        }
       
        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open file clicked");
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HelpGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("User guide coming soon...");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string aboutText = "Job Management System v1.0\n© 2025 Created By Mohammed Rameez P P\nAll rights reserved.";
            MessageBox.Show(aboutText, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}

