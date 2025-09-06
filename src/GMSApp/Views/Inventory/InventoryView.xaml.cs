using GMSApp.Models.inventory;
using GMSApp.ViewModels.Inventory;
using GMSApp.ViewModels.Job;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GMSApp.Views.Inventory
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
        }
        public InventoryView(InventoryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
        private async void AddTxn_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is InventoryViewModel vm))
                return;

            if (vm.SelectedItem == null)
            {
                MessageBox.Show("Select an inventory item first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxnQty.Text, out var qty))
            {
                MessageBox.Show("Invalid quantity.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxnPrice.Text, out var price))
            {
                MessageBox.Show("Invalid unit price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = TxnType.SelectedItem as ComboBoxItem;
            if (selected == null)
            {
                MessageBox.Show("Select transaction type.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Enum.TryParse(selected.Tag?.ToString(), out StockTransactionType type))
            {
                MessageBox.Show("Invalid transaction type.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editable = new InventoryViewModel.EditableStockTransaction
            {
                TransactionDate = DateTime.UtcNow,
                TransactionType = type,
                Quantity = qty,
                UnitPrice = price,
                Notes = TxnNotes.Text
            };

            await vm.AddTransactionAsync(editable);
        }
    }
}