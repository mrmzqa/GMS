
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
    public partial class InventoryItemViewModel : ObservableObject
    {
        private readonly IRepository<InventoryItem> _itemRepo;

        public ObservableCollection<InventoryItem> Items { get; } = new();

        public InventoryItemViewModel(IRepository<InventoryItem> itemRepo)
        {
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private InventoryItem? selectedItem;

        partial void OnSelectedItemChanged(InventoryItem? value)
        {
            // ensure non-null references for binding (avoid null Address-like issues)
            if (value != null)
            {
                // nothing special here, but could ensure collections exist
            }

            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Items.Clear();
                var list = await _itemRepo.GetAllAsync();
                foreach (var it in list) Items.Add(it);
                SelectedItem = Items.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load inventory items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var it = new InventoryItem
            {
                ItemCode = $"IT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = string.Empty,
                Description = string.Empty,
                Category = string.Empty,
                SubCategory = string.Empty,
                Unit = "pcs",
                QuantityInStock = 0,
                ReorderLevel = 5,
                LastRestocked = DateTime.UtcNow
            };
            Items.Add(it);
            SelectedItem = it;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedItem == null) return;

            try
            {
                var detached = new InventoryItem
                {
                    Id = SelectedItem.Id,
                    ItemCode = SelectedItem.ItemCode?.Trim() ?? string.Empty,
                    Name = SelectedItem.Name?.Trim() ?? string.Empty,
                    Description = SelectedItem.Description,
                    Category = SelectedItem.Category,
                    SubCategory = SelectedItem.SubCategory,
                    QuantityInStock = SelectedItem.QuantityInStock,
                    ReorderLevel = SelectedItem.ReorderLevel,
                    Unit = SelectedItem.Unit,
                    CostPrice = SelectedItem.CostPrice,
                    SellingPrice = SelectedItem.SellingPrice,
                    Currency = SelectedItem.Currency,
                    VendorId = SelectedItem.VendorId,
                    Location = SelectedItem.Location,
                    LastRestocked = SelectedItem.LastRestocked,
                    IsActive = SelectedItem.IsActive
                };

                if (detached.Id == 0)
                    await _itemRepo.AddAsync(detached);
                else
                    await _itemRepo.UpdateAsync(detached);

                await LoadAsync();
                SelectedItem = Items.FirstOrDefault(i => i.ItemCode == detached.ItemCode) ?? SelectedItem;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            var confirm = MessageBox.Show("Delete selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedItem.Id == 0)
                {
                    Items.Remove(SelectedItem);
                    SelectedItem = Items.FirstOrDefault();
                }
                else
                {
                    await _itemRepo.DeleteAsync(SelectedItem.Id);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedItem != null;
    }
}

<UserControl x:Class="GMSApp.Views.Inventory.InventoryItemView"
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
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0,0,0,8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Items}"
                          SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Code" Binding="{Binding ItemCode}" Width="120"/>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
                        <DataGridTextColumn Header="Stock" Binding="{Binding QuantityInStock}" Width="90"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" ShowsPreview="True"/>
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Code:" Width="140" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.ItemCode, Mode=TwoWay}" Width="260"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Name:" Width="140" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.Name, Mode=TwoWay}" Width="360"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Category:" Width="140" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.Category, Mode=TwoWay}" Width="200"/>
                    <TextBlock Text="Unit:" Width="60" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.Unit, Mode=TwoWay}" Width="80"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="QuantityInStock:" Width="140" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.QuantityInStock, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="120"/>
                    <TextBlock Text="Reorder level:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.ReorderLevel, Mode=TwoWay}" Width="80"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Cost price:" Width="140" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.CostPrice, Mode=TwoWay, StringFormat=N2}" Width="140"/>
                    <TextBlock Text="Sell price:" Width="100" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.SellingPrice, Mode=TwoWay, StringFormat=N2}" Width="140"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
                    <TextBlock Text="Last restocked:" VerticalAlignment="Center" Margin="0,0,8,0"/>
                    <TextBlock Text="{Binding SelectedItem.LastRestocked, StringFormat=\{0:yyyy-MM-dd\}}" VerticalAlignment="Center"/>
                </StackPanel>
            </StackPanel>
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
    public partial class PurchaseOrderViewModel : ObservableObject
    {
        private readonly IRepository<PurchaseOrder> _poRepo;
        private readonly IRepository<InventoryItem> _itemRepo;
        private readonly IRepository<StockTransaction> _txnRepo;

        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public PurchaseOrderViewModel(IRepository<PurchaseOrder> poRepo,
                                      IRepository<InventoryItem> itemRepo,
                                      IRepository<StockTransaction> txnRepo)
        {
            _poRepo = poRepo ?? throw new ArgumentNullException(nameof(poRepo));
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private PurchaseOrder? selectedPurchaseOrder;
        public ObservableCollection<EditablePOItem> POItems { get; } = new();

        partial void OnSelectedPurchaseOrderChanged(PurchaseOrder? value)
        {
            POItems.Clear();
            if (value != null)
            {
                foreach (var it in value.Items ?? Enumerable.Empty<PurchaseOrderItem>())
                {
                    var ed = new EditablePOItem(it);
                    ed.PropertyChanged += ItemPropChanged;
                    POItems.Add(ed);
                }
            }
            LoadCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            ReceiveCommand.NotifyCanExecuteChanged();
        }

        private void ItemPropChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditablePOItem.Quantity) || e.PropertyName == nameof(EditablePOItem.UnitPrice))
                UpdateTotals();
        }

        [ObservableProperty] private decimal totalAmount;

        private void UpdateTotals()
        {
            TotalAmount = POItems.Sum(i => i.Quantity * i.UnitPrice);
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                PurchaseOrders.Clear();
                InventoryItems.Clear();

                var list = await _poRepo.GetAllAsync();
                foreach (var po in list) PurchaseOrders.Add(po);

                var inv = await _itemRepo.GetAllAsync();
                foreach (var i in inv) InventoryItems.Add(i);

                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load purchase orders/items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var po = new PurchaseOrder
            {
                OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                OrderDate = DateTime.UtcNow,
                Status = PurchaseOrderStatus.Pending,
                Items = new System.Collections.Generic.List<PurchaseOrderItem>()
            };
            PurchaseOrders.Add(po);
            SelectedPurchaseOrder = po;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                // map editable items to detached PO
                var detached = new PurchaseOrder
                {
                    Id = SelectedPurchaseOrder.Id,
                    OrderNumber = SelectedPurchaseOrder.OrderNumber,
                    VendorId = SelectedPurchaseOrder.VendorId,
                    OrderDate = SelectedPurchaseOrder.OrderDate,
                    ExpectedDeliveryDate = SelectedPurchaseOrder.ExpectedDeliveryDate,
                    Status = SelectedPurchaseOrder.Status,
                    Currency = SelectedPurchaseOrder.Currency,
                    Items = POItems.Select(p => new PurchaseOrderItem
                    {
                        Id = p.Id,
                        InventoryItemId = p.InventoryItemId,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    }).ToList()
                };

                detached.TotalAmount = detached.Items.Sum(i => i.Quantity * i.UnitPrice);

                if (detached.Id == 0) await _poRepo.AddAsync(detached);
                else await _poRepo.UpdateAsync(detached);

                await LoadAsync();
                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(x => x.OrderNumber == detached.OrderNumber) ?? SelectedPurchaseOrder;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save PO: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Mark PO as Received: create StockTransactions and update InventoryItem.QuantityInStock
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task ReceiveAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                // If already received skip
                if (SelectedPurchaseOrder.Status == PurchaseOrderStatus.Received)
                {
                    MessageBox.Show("Purchase order already received.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Build stock transactions for each item
                foreach (var p in SelectedPurchaseOrder.Items ?? Enumerable.Empty<PurchaseOrderItem>())
                {
                    // create stock txn
                    var txn = new StockTransaction
                    {
                        InventoryItemId = p.InventoryItemId,
                        TransactionDate = DateTime.UtcNow,
                        TransactionType = StockTransactionType.Purchase,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice,
                        PurchaseOrderId = SelectedPurchaseOrder.Id,
                        Notes = $"PO Receive {SelectedPurchaseOrder.OrderNumber}"
                    };

                    await _txnRepo.AddAsync(txn);

                    // update inventory item stock (load item, apply increment, save via repository)
                    var item = (await _itemRepo.GetAllAsync()).FirstOrDefault(i => i.Id == p.InventoryItemId);
                    if (item != null)
                    {
                        item.QuantityInStock += p.Quantity;
                        item.LastRestocked = DateTime.UtcNow;
                        await _itemRepo.UpdateAsync(item);
                    }
                }

                // update PO status
                SelectedPurchaseOrder.Status = PurchaseOrderStatus.Received;
                await _poRepo.UpdateAsync(new PurchaseOrder
                {
                    Id = SelectedPurchaseOrder.Id,
                    Status = SelectedPurchaseOrder.Status
                });

                await LoadAsync();
                MessageBox.Show("Purchase order received and stock updated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to receive PO: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedPurchaseOrder == null) return;
            var confirm = MessageBox.Show("Delete selected PO?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedPurchaseOrder.Id == 0)
                {
                    PurchaseOrders.Remove(SelectedPurchaseOrder);
                    SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault();
                }
                else
                {
                    await _poRepo.DeleteAsync(SelectedPurchaseOrder.Id);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete PO: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Editable PO items for UI
        public class EditablePOItem : ObservableObject
        {
            public EditablePOItem() { }

            public EditablePOItem(PurchaseOrderItem src)
            {
                Id = src.Id;
                InventoryItemId = src.InventoryItemId;
                Quantity = src.Quantity;
                UnitPrice = src.UnitPrice;
            }

            [ObservableProperty] private int id;
            [ObservableProperty] private int inventoryItemId;
            partial void OnInventoryItemIdChanged(int value) => OnPropertyChanged(nameof(InventoryItemId));
            [ObservableProperty] private int quantity;
            partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(LineTotal));
            [ObservableProperty] private decimal unitPrice;
            partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
            public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddItem()
        {
            POItems.Add(new EditablePOItem { Quantity = 1, UnitPrice = 0m });
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveItem(EditablePOItem? itm)
        {
            if (itm == null) return;
            POItems.Remove(itm);
            UpdateTotals();
        }

        private bool CanModify() => SelectedPurchaseOrder != null;
    }
}

<UserControl x:Class="GMSApp.Views.Inventory.PurchaseOrderView"
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
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                    <Button Content="Receive" Command="{Binding ReceiveCommand}" Width="80" Margin="6,0,0,0"/>
                </StackPanel>

                <DataGrid ItemsSource="{Binding PurchaseOrders}"
                          SelectedItem="{Binding SelectedPurchaseOrder, Mode=TwoWay}"
                          AutoGenerateColumns="False" MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="PO #" Binding="{Binding OrderNumber}" Width="*"/>
                        <DataGridTextColumn Header="Date" Binding="{Binding OrderDate, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="VendorId" Binding="{Binding VendorId}" Width="90"/>
                        <DataGridTextColumn Header="Status" Binding="{Binding Status}" Width="110"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" ShowsPreview="True"/>
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <StackPanel Grid.Row="0" Orientation="Horizontal">
                    <TextBlock Text="PO #" Width="100" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedPurchaseOrder.OrderNumber, Mode=TwoWay}" Width="220"/>
                    <TextBlock Text="Order Date:" Width="90" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedPurchaseOrder.OrderDate, Mode=TwoWay}" />
                </StackPanel>

                <GroupBox Grid.Row="2" Header="Items" Padding="6">
                    <DockPanel LastChildFill="True">
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0,0,0,6">
                            <Button Content="Add Item" Command="{Binding AddItemCommand}" Width="90" Margin="0,0,6,0"/>
                            <Button Content="Remove Item" Command="{Binding RemoveItemCommand}" CommandParameter="{Binding SelectedItem, ElementName=POItemsGrid}" Width="110"/>
                        </StackPanel>

                        <DataGrid x:Name="POItemsGrid" ItemsSource="{Binding POItems}" AutoGenerateColumns="False" CanUserAddRows="False">
                            <DataGrid.Columns>
                                <DataGridComboBoxColumn Header="Item"
                                                        SelectedValueBinding="{Binding InventoryItemId, Mode=TwoWay}"
                                                        SelectedValuePath="Id"
                                                        DisplayMemberPath="Name"
                                                        ItemsSource="{Binding DataContext.InventoryItems, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                                        Width="300"/>
                                <DataGridTextColumn Header="Qty" Binding="{Binding Quantity, Mode=TwoWay}" Width="80"/>
                                <DataGridTextColumn Header="Unit Price" Binding="{Binding UnitPrice, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                                <DataGridTextColumn Header="Line Total" Binding="{Binding LineTotal, Mode=OneWay, StringFormat=N2}" Width="120" IsReadOnly="True"/>
                            </DataGrid.Columns>
                        </DataGrid>
                    </DockPanel>
                </GroupBox>

                <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right">
                    <TextBlock Text="Total:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,0,8,0"/>
                    <TextBlock Text="{Binding TotalAmount, StringFormat=N2}" FontWeight="Bold" VerticalAlignment="Center"/>
                </StackPanel>
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
    public partial class StockTransactionViewModel : ObservableObject
    {
        private readonly IRepository<StockTransaction> _txnRepo;
        private readonly IRepository<InventoryItem> _itemRepo;

        public ObservableCollection<StockTransaction> Transactions { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        public StockTransactionViewModel(IRepository<StockTransaction> txnRepo, IRepository<InventoryItem> itemRepo)
        {
            _txnRepo = txnRepo ?? throw new ArgumentNullException(nameof(txnRepo));
            _itemRepo = itemRepo ?? throw new ArgumentNullException(nameof(itemRepo));
            _ = LoadAsync();
        }

        [ObservableProperty] private StockTransaction? selectedTransaction;

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Transactions.Clear();
                InventoryItems.Clear();

                var txns = await _txnRepo.GetAllAsync();
                foreach (var t in txns) Transactions.Add(t);

                var items = await _itemRepo.GetAllAsync();
                foreach (var i in items) InventoryItems.Add(i);

                SelectedTransaction = Transactions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load transactions/items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add manual transaction (adjustment/return)
        [RelayCommand]
        public async Task AddAdjustmentAsync(int inventoryItemId, StockTransactionType type, int quantity, decimal unitPrice, string notes)
        {
            try
            {
                var txn = new StockTransaction
                {
                    InventoryItemId = inventoryItemId,
                    TransactionType = type,
                    TransactionDate = DateTime.UtcNow,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Notes = notes
                };

                await _txnRepo.AddAsync(txn);

                // update stock level: Purchase/Return increase, JobUsage/Adjustment decrease (Adjustment can be negative or positive depending on sign)
                var item = (await _itemRepo.GetAllAsync()).FirstOrDefault(i => i.Id == inventoryItemId);
                if (item != null)
                {
                    switch (type)
                    {
                        case StockTransactionType.Purchase:
                        case StockTransactionType.Return:
                            item.QuantityInStock += quantity;
                            break;
                        case StockTransactionType.JobUsage:
                            item.QuantityInStock -= quantity;
                            break;
                        case StockTransactionType.Adjustment:
                            // For adjustment, we accept signed quantity. Caller should pass negative to reduce stock.
                            item.QuantityInStock += quantity;
                            break;
                    }

                    item.UpdatedAt = DateTime.UtcNow;
                    await _itemRepo.UpdateAsync(item);
                }

                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedTransaction == null) return;
            var confirm = MessageBox.Show("Delete selected transaction? This will NOT revert stock automatically.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedTransaction.Id == 0)
                {
                    Transactions.Remove(SelectedTransaction);
                }
                else
                {
                    await _txnRepo.DeleteAsync(SelectedTransaction.Id);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedTransaction != null;
    }
}

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
