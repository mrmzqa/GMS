
using GMSApp.ViewModels;
using System.Windows.Controls;

namespace GmsApp.Views
{
    public partial class MainContentView : UserControl
    {
        private readonly MainContentViewModel _viewModel;

        public MainContentView(MainContentViewModel viewModel)
        {

            InitializeComponent();
            DataContext = viewModel;
        }
    }
}