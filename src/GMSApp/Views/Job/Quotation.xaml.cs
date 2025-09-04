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
    /// <summary>
    /// Interaction logic for Quotation.xaml
    /// </summary>
    public partial class Quotation : UserControl
    {
        public Quotation()
        {
            InitializeComponent();
        }
        public Quotation(QuotationViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
