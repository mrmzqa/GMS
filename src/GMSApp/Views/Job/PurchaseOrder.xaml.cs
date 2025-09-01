using GMSApp.ViewModels;
using GMSApp.ViewModels;
using GMSApp.ViewModels.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GMSApp.Views.Job
{
    public partial class PurchaseOrder : UserControl
    {
        public PurchaseOrder()
        {
            InitializeComponent();
        }

        // Optional constructor for DI
        public PurchaseOrder(PurchaseorderViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
