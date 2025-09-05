

<UserControl x:Class="GMSApp.Views.Inventory.StockTransactionView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="600" d:DesignWidth="900">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360"/>
            <ColumnDefinition Width="12"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" Background="WhiteSmoke" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Transactions}" SelectedItem="{Binding SelectedTransaction, Mode=TwoWay}" AutoGenerateColumns="False" MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Date" Binding="{Binding TransactionDate, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="Item" Binding="{Binding InventoryItem.Name}" Width="220"/>
                        <DataGridTextColumn Header="Type" Binding="{Binding TransactionType}" Width="110"/>
                        <DataGridTextColumn Header="Qty" Binding="{Binding Quantity}" Width="90"/>
                        <DataGridTextColumn Header="UnitPrice" Binding="{Binding UnitPrice, StringFormat=N2}" Width="110"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4"/>
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <StackPanel>
                <TextBlock Text="Add Manual Adjustment / Return" FontWeight="Bold" Margin="0,0,0,8"/>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                    <TextBlock Text="Item:" Width="120" VerticalAlignment="Center"/>
                    <ComboBox ItemsSource="{Binding InventoryItems}" DisplayMemberPath="Name" SelectedValuePath="Id" x:Name="ItemCombo" Width="300"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                    <TextBlock Text="Type:" Width="120" VerticalAlignment="Center"/>
                    <ComboBox x:Name="TypeCombo" Width="200">
                        <ComboBoxItem Tag="Purchase">Purchase</ComboBoxItem>
                        <ComboBoxItem Tag="JobUsage">JobUsage</ComboBoxItem>
                        <ComboBoxItem Tag="Adjustment">Adjustment</ComboBoxItem>
                        <ComboBoxItem Tag="Return">Return</ComboBoxItem>
                    </ComboBox>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                    <TextBlock Text="Quantity:" Width="120" VerticalAlignment="Center"/>
                    <TextBox x:Name="QtyBox" Width="120" Text="1"/>
                    <TextBlock Text="Unit Price:" Width="100" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox x:Name="PriceBox" Width="120" Text="0.00"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,8,0,0">
                    <TextBlock Text="Notes:" Width="120" VerticalAlignment="Top"/>
                    <TextBox x:Name="NotesBox" Width="420" Height="60" TextWrapping="Wrap"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
                    <Button Content="Add" Width="100" Click="Add_Click"/>
                </StackPanel>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
using System;
using System.Windows;
using System.Windows.Controls;
using GMSApp.ViewModels.Inventory;

namespace GMSApp.Views.Inventory
{
    public partial class StockTransactionView : UserControl
    {
        public StockTransactionView()
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
<UserControl x:Class="GMSApp.Views.Inventory.JobUsageView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="700" d:DesignWidth="1100">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360"/>
            <ColumnDefinition Width="12"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" Background="WhiteSmoke" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Use Items" Command="{Binding UseItemsCommand}" Width="100"/>
                </StackPanel>

                <DataGrid ItemsSource="{Binding JobOrders}" SelectedItem="{Binding SelectedJob, Mode=TwoWay}" AutoGenerateColumns="False" MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Job #" Binding="{Binding Id}" Width="80"/>
                        <DataGridTextColumn Header="Customer" Binding="{Binding CustomerName}" Width="180"/>
                        <DataGridTextColumn Header="Vehicle" Binding="{Binding VehicleNumber}" Width="140"/>
                    </DataGrid.Columns>
                </DataGrid>
            </StackPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4"/>
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Job #" Width="80" VerticalAlignment="Center"/>
                    <TextBlock Text="{Binding SelectedJob.Id}" VerticalAlignment="Center"/>
                    <TextBlock Text="Customer:" Width="80" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBlock Text="{Binding SelectedJob.CustomerName}" VerticalAlignment="Center"/>
                </StackPanel>

                <GroupBox Grid.Row="2" Header="Items to use" Padding="6">
                    <DockPanel LastChildFill="True">
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0,0,0,6">
                            <Button Content="Add Item" Command="{Binding AddItemCommand}" Width="90" Margin="0,0,6,0"/>
                            <Button Content="Remove Item" Command="{Binding RemoveItemCommand}" CommandParameter="{Binding SelectedItem, ElementName=JobItemsGrid}" Width="100"/>
                        </StackPanel>

                        <DataGrid x:Name="JobItemsGrid" ItemsSource="{Binding JobItems}" AutoGenerateColumns="False" CanUserAddRows="False">
                            <DataGrid.Columns>
                                <DataGridComboBoxColumn Header="Item"
                                                        SelectedValueBinding="{Binding InventoryItemId, Mode=TwoWay}"
                                                        SelectedValuePath="Id"
                                                        DisplayMemberPath="Name"
                                                        ItemsSource="{Binding DataContext.InventoryItems, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                                        Width="360"/>
                                <DataGridTextColumn Header="Qty" Binding="{Binding QuantityUsed, Mode=TwoWay}" Width="100"/>
                                <DataGridTextColumn Header="UnitPrice" Binding="{Binding UnitPrice, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                                <DataGridTextColumn Header="Total" Binding="{Binding Total, Mode=OneWay, StringFormat=N2}" Width="120" IsReadOnly="True"/>
                            </DataGrid.Columns>
                        </DataGrid>
                    </DockPanel>
                </GroupBox>
            </Grid>
        </Border>
    </Grid>
</UserControl>
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Inventory
{
    public partial class JobUsageViewModel : ObservableObject
    {
        private readonly IRepository<JobOrder> _jobRepo;
        private readonly IRepository<InventoryItem> _itemRepo;
        private readonly IRepository<StockTransaction> _txnRepo;
        private readonly IRepository<JobOrderItem> _jobItemRepo; // optional if you persist job usage separately

        public ObservableCollection<JobOrder> JobOrders { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public JobUsageViewModel(IRepository<JobOrder> jobRepo,
                                 IRepository<InventoryItem> itemRepo,
                                 IRepository<StockTransaction> txnRepo,
                                 IRepository<JobOrderItem> jobItemRepo)
        {
            _jobRepo = jobRepo ?? throw new ArgumentNullException(nameof(jobRepo));
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));
            _jobItemRepo = jobItemRepo ?? throw new ArgumentNullException(nameof(jobItemRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private JobOrder? selectedJob;
        public ObservableCollection<JobOrderItemEditable> JobItems { get; } = new();

        partial void OnSelectedJobChanged(JobOrder? value)
        {
            JobItems.Clear();
            if (value != null)
            {
                // if job has existing items, map them here (not required)
            }

            LoadCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                JobOrders.Clear();
                InventoryItems.Clear();

                var jobs = await _jobRepo.GetAllAsync();
                foreach (var j in jobs) JobOrders.Add(j);

                var inv = await _itemRepo.GetAllAsync();
                foreach (var i in inv) InventoryItems.Add(i);

                SelectedJob = JobOrders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load jobs/items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UseItemsAsync()
        {
            if (SelectedJob == null) return;
            if (!JobItems.Any()) { MessageBox.Show("Add items to use for this job.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            try
            {
                // persist job items and create stock txns
                foreach (var ji in JobItems)
                {
                    // create job order item (persist if repo is available)
                    var jobItem = new JobOrderItem
                    {
                        JobOrderId = SelectedJob.Id,
                        InventoryItemId = ji.InventoryItemId,
                        QuantityUsed = ji.QuantityUsed,
                        UnitPrice = ji.UnitPrice
                    };

                    await _jobItemRepo.AddAsync(jobItem);

                    var txn = new StockTransaction
                    {
                        InventoryItemId = ji.InventoryItemId,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = StockTransactionType.JobUsage,
                        Quantity = ji.QuantityUsed,
                        UnitPrice = ji.UnitPrice,
                        JobOrderId = SelectedJob.Id,
                        Notes = $"Used in Job {SelectedJob.Id}"
                    };

                    await _txnRepo.AddAsync(txn);

                    // update inventory
                    var item = (await _itemRepo.GetAllAsync()).FirstOrDefault(i => i.Id == ji.InventoryItemId);
                    if (item != null)
                    {
                        item.QuantityInStock -= ji.QuantityUsed;
                        item.UpdatedAt = DateTime.UtcNow;
                        await _itemRepo.UpdateAsync(item);
                    }
                }

                await LoadAsync();
                MessageBox.Show("Items consumed and stock updated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to use items for job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddItem()
        {
            JobItems.Add(new JobOrderItemEditable { QuantityUsed = 1, UnitPrice = 0m });
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveItem(JobOrderItemEditable? it)
        {
            if (it == null) return;
            JobItems.Remove(it);
        }

        private bool CanModify() => SelectedJob != null;

        public class JobOrderItemEditable : ObservableObject
        {
            [ObservableProperty] private int inventoryItemId;
            [ObservableProperty] private int quantityUsed;
            partial void OnQuantityUsedChanged(int value) => OnPropertyChanged(nameof(Total));
            [ObservableProperty] private decimal unitPrice;
            partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(Total));
            public decimal Total => Math.Round(QuantityUsed * UnitPrice, 2);
        }
    }
}
