
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
        private readonly Invoice _invoice;
        private readonly Quotation _quotation;

        public JobContentView(JobContentViewModel viewModel,PurchaseOrder purchaseOrder,JobOrder jobOrder , Invoice invoice, Quotation quotation  )
        {

            InitializeComponent();
            DataContext = viewModel;
            _purchaseOrder = purchaseOrder;
            _jobOrder = jobOrder;
            _invoice = invoice;
            _quotation = quotation;
        }

      

        private void Jo_Click(object sender, RoutedEventArgs e)
        {

            JobContent.Content = _jobOrder;

        }
        private void Po_Click(object sender, RoutedEventArgs e)
        {

            POContent.Content = _purchaseOrder;

        }
        private void Q_Click(object sender, RoutedEventArgs e)
        {

            QContent.Content = _quotation;

        }
        private void I_Click(object sender, RoutedEventArgs e)
        {

            IContent.Content = _invoice;

        }
    }

}