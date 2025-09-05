using GMSApp.Models.inventory;
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
    /// Interaction logic for StockTransaction.xaml
    /// </summary>
    public partial class StockTransaction : UserControl
    {
        public StockTransaction()
        {
            InitializeComponent();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is StockTransactionViewModel vm)) return;

            if (ItemCombo.SelectedValue == null)
            {
                MessageBox.Show("Select an item first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(int.TryParse(QtyBox.Text, out int qty))) { MessageBox.Show("Invalid quantity."); return; }
            if (!(decimal.TryParse(PriceBox.Text, out decimal price))) { MessageBox.Show("Invalid unit price."); return; }

            var selectedTag = (TypeCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (!Enum.TryParse(selectedTag, out StockTransactionType type))
            {
                MessageBox.Show("Select a valid type.");
                return;
            }

            await vm.AddAdjustmentAsync((int)ItemCombo.SelectedValue, type, qty, price, NotesBox.Text ?? string.Empty);
        }
    }
}
