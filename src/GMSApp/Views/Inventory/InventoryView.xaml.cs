using GMSApp.Models.inventory;
using GMSApp.ViewModels.Inventory;
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

        // Quick-entry handler: ensures the SelectedItem is persisted before creating a transaction,
        // validates inputs and calls the ViewModel command to add the transaction.
        private async void AddTxn_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is InventoryViewModel vm))
                return;

            if (vm.SelectedItem == null)
            {
                MessageBox.Show("Select an inventory item first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxnQty.Text, out var qty) || qty == 0)
            {
                MessageBox.Show("Invalid quantity. Use non-zero integer.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            // If SelectedItem is new (Id == 0) we must persist it first so the transaction has a valid InventoryItemId
            if (vm.SelectedItem.Id == 0)
            {
                var saveConfirm = MessageBox.Show("This item is new and must be saved before adding a transaction. Save now?", "Save item", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (saveConfirm != MessageBoxResult.Yes) return;

                await vm.SaveItemAsync();

                // after save, ensure we have a persisted item
                if (vm.SelectedItem == null || vm.SelectedItem.Id == 0)
                {
                    MessageBox.Show("Failed to persist the new item. Cannot add transaction.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var editable = new InventoryViewModel.EditableStockTransaction
            {