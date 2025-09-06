using GMSApp.ViewModels.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GMSApp.Views.Inventory
{
    /// <summary>
    /// Interaction logic for JobUsage.xaml
    /// </summary>
    public partial class JobUsage : UserControl
    {
        public JobUsage()
        {
            InitializeComponent();
        }


        // Use this ctor when resolving the VM via DI so DataContext is set automatically.
        public JobUsage(JobUsageViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
