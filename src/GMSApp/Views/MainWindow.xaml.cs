using GMSApp.ViewModels;
using GMSApp.Views;
using System.Windows;

namespace GMSApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly FilesPage _filesPage;
        private readonly CoreMain _coreMain;
        private readonly ProductView _productView;
        private readonly CategoryView _categoryView;

        public MainWindow(FilesPage filesPage, CoreMain coreMain, ProductView productView, CategoryView categoryView)
        {
            InitializeComponent();
            _filesPage = filesPage;
            _coreMain = coreMain;
            _productView = productView;
            _categoryView = categoryView;
        }

        private void CoreMainButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _coreMain;
        }

        private void FilesButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _filesPage;
        }
        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _productView;
        }
        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _categoryView;
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

