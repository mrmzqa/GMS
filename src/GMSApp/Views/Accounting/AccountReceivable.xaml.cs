using GMSApp.ViewModels.Accounting;
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

namespace GMSApp.Views.Accounting
{
    /// <summary>
    /// Interaction logic for AccountReceivable.xaml
    /// </summary>
    public partial class AccountReceivable : UserControl
    {
        public AccountReceivable()
        {
            InitializeComponent();
        }

        public AccountReceivable(AccountsReceivableViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
