using GMSApp.ViewModels.Job;
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

namespace GMSApp.Views.Job
{

    public partial class JobOrder : UserControl
    {
        // Parameterless ctor for designer support
        public JobOrder()
        {
            InitializeComponent();
        }

        // Use this constructor when composing from DI (e.g. in a window or a view locator)
        public JobOrder(JoborderViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
