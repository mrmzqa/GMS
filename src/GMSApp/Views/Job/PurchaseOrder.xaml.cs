// File: Views/PurchaseOrderView.xaml.cs
using GMSApp.ViewModels;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class PurchaseOrderView : UserControl
    {
        public PurchaseOrderView()
        {
            InitializeComponent();
        }

        // If using DI, you can inject the VM via constructor and set DataContext
        public PurchaseOrderView(PurchaseOrderViewModel vm) : this()
        {
            DataContext = vm;
        }
    }
}