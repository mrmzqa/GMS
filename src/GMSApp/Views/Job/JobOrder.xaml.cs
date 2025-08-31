

using GMSApp.ViewModels.Job;
using System.Windows.Controls;

namespace GMSApp.Views.Job
{
    public partial class JobOrder : UserControl
    {
        public JobOrder()
        {
            InitializeComponent();
        }

        // Use this ctor when resolving the VM via DI so DataContext is set automatically.
        public JobOrder(JoborderViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}