using GMSApp.ViewModels;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class Garage : UserControl
    {
        public Garage( GarageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
          
            
        }
    }
}