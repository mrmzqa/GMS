// File: Views/PurchaseOrderView.xaml.cs
using GMSApp.ViewModels;
using GMSApp.ViewModels.Job;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class VendorView : UserControl
    {
        public VendorView()
        {
            InitializeComponent();
        }

        // If using DI, you can inject the VM via constructor and set DataContext
        public VendorView(VendorViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}