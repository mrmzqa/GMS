// File: Views/PurchaseOrderView.xaml.cs
using GMSApp.ViewModels;
using GMSApp.ViewModels.Job;
using System.Windows.Controls;

namespace GMSApp.Views.Job
{
    public partial class PurchaseOrder : UserControl
    {
        public PurchaseOrder()
        {
            InitializeComponent();
        }

        // If using DI, you can inject the VM via constructor and set DataContext
        public PurchaseOrder(PurchaseOrderViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}