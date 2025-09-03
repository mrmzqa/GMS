
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using GMSApp.ViewModels.Job;

namespace GMSApp.Views.Job
{
    public partial class PurchaseOrder : UserControl
    {
        public PurchaseOrder()
        {
            InitializeComponent();
        }
        public PurchaseOrder(PurchaseOrderViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        // Called when the vendor ComboBox is opened
        private async void VendorComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (DataContext is PurchaseOrderViewModel vm)
            {
                // Load vendors only if not loaded, or pass true to force reload
                await vm.LoadVendorsAsync();
            }
        }

        // If you have a Cancel button Click handler as in the earlier XAML, keep it here.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // example: simply reload list or clear selection
            if (DataContext is PurchaseOrderViewModel vm)
            {
                vm.SelectedPurchaseOrder = null;
            }
        }
    }
}