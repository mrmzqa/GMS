using GMSApp.ViewModels;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class CoreMain : UserControl
    {
        public CoreMain(CoreMainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}