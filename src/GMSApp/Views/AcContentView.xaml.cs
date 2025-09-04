using GMSApp.ViewModels;
using GMSApp.Views.Accounting;
using PdfSharpCore.Pdf.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
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

namespace GMSApp.Views
{
    /// <summary>
    /// Interaction logic for AcContentView.xaml
    /// </summary>
    public partial class AcContentView : UserControl
    {

        private readonly AccountPayable _accountPayable;

        private readonly AccountReceivable _accountReceivable;
        private readonly AccountReconcile _accountReconcile;

        private readonly ChartofAccount _chartofAccount;
         private readonly GeneralLedger _generalLedger;
           



        public AcContentView(  AccountPayable accountPayable, AccountReceivable accountReceivable, AccountReconcile accountReconcile, ChartofAccount chartofAccount, GeneralLedger generalLedger  )
        {
            InitializeComponent();
       
            _generalLedger = generalLedger;  
            _chartofAccount = chartofAccount;
            _accountReconcile = accountReconcile;
            _accountReceivable = accountReceivable;
            _accountPayable = accountPayable;

        }


        private void Ap_Click(object sender, RoutedEventArgs e)
        {

            ApContent.Content = _accountPayable;

        }
        private void Ar_Click(object sender, RoutedEventArgs e)
        {

            ArContent.Content = _accountReceivable;

        }
        private void Ac_Click(object sender, RoutedEventArgs e)
        {

            AcContent.Content = _accountReconcile;

        }
        private void G_Click(object sender, RoutedEventArgs e)
        {

            GLContent.Content = _generalLedger;

        }
        private void Ch_Click(object sender, RoutedEventArgs e)
        {

            CHContent.Content = _chartofAccount;

        }
    }
}
