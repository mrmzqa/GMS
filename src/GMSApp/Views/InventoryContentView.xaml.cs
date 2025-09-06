using GMSApp.Views.Inventory;
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
    /// Interaction logic for InventoryContentView.xaml
    /// </summary>
    public partial class InventoryContentView : UserControl
    {
        private readonly JobUsage _jobUsage;
        private readonly InventoryItem _inventoryItem;
        private readonly StockTransaction _stockTransaction;


        public InventoryContentView(InventoryItem inventoryItem, StockTransaction stockTransaction, JobUsage jobUsage)
        {
            InitializeComponent();
            _inventoryItem = inventoryItem;
            _stockTransaction = stockTransaction;
            _jobUsage = jobUsage;
         
        }
        private void J_Click(object sender, RoutedEventArgs e)
        {

            InvContent.Content = _jobUsage;

        }
        private void S_Click(object sender, RoutedEventArgs e)
        {

            SContent.Content = _stockTransaction;

        }
        private void I_Click(object sender, RoutedEventArgs e)
        {

            InvContent.Content = _inventoryItem;

        }
    }
}
