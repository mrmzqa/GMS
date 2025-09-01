
using GMSApp.ViewModels;
using GMSApp.Views;
using GMSApp.Views.Job;
using System.Windows;
using System.Windows.Controls;

namespace GmsApp.Views
{
    public partial class JobContentView : UserControl
    {
        private readonly JobContentViewModel _viewModel;
        private readonly PurchaseOrder _purchaseOrder;
        private readonly JobOrder _jobOrder;

        public JobContentView(JobContentViewModel viewModel,PurchaseOrder purchaseOrder,JobOrder jobOrder )
        {

            InitializeComponent();
            DataContext = viewModel;
            _purchaseOrder = purchaseOrder;
            _jobOrder = jobOrder;
        }

      

        private void Jo_Click(object sender, RoutedEventArgs e)
        {

            JobContent.Content = _jobOrder;

        }
        private void Po_Click(object sender, RoutedEventArgs e)
        {

            POContent.Content = _purchaseOrder;

        }
    }

}