
using GMSApp.ViewModels;
using GMSApp.Views.Job;
using System.Windows;
using System.Windows.Controls;

namespace GmsApp.Views
{
    public partial class MainContentView : UserControl
    {
        private readonly MainContentViewModel _viewModel;
        private readonly PurchaseOrderPage _purchaseOrderPage;

        public MainContentView(MainContentViewModel viewModel,PurchaseOrderPage purchaseOrderPage)
        {

            InitializeComponent();
            DataContext = viewModel;
            _purchaseOrderPage = purchaseOrderPage;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void TreeViewItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}