<UserControl x:Class="GMSApp.Views.Accounting.AccountReconciliationView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="700" d:DesignWidth="1100">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360"/>
            <ColumnDefinition Width="12"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0" />
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0" />
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0" />
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Reconciliations}"
                          SelectedItem="{Binding SelectedReconciliation, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          IsReadOnly="False"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Date" Binding="{Binding ReconciliationDate, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="Statement Bal" Binding="{Binding StatementBalance, StringFormat=N2}" Width="120"/>
                        <DataGridTextColumn Header="Ledger Bal" Binding="{Binding LedgerBalance, StringFormat=N2}" Width="120"/>
                        <DataGridCheckBoxColumn Header="Reconciled" Binding="{Binding IsReconciled}" Width="100"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

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
                    <TextBlock Text="AccountId:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedReconciliation.ChartOfAccountId, Mode=TwoWay}" Width="120"/>
                    <TextBlock Text="Date:" Width="80" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedReconciliation.ReconciliationDate, Mode=TwoWay}" />
                </StackPanel>

                <StackPanel Grid.Row="2" Orientation="Vertical">
                    <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                        <TextBlock Text="Statement Balance:" Width="140" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding SelectedReconciliation.StatementBalance, Mode=TwoWay, StringFormat=N2}" Width="160"/>
                        <TextBlock Text="Ledger Balance:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                        <TextBlock Text="{Binding LedgerBalance, StringFormat=N2}" VerticalAlignment="Center" FontWeight="Bold"/>
                        <TextBlock Text="Reconciled:" Width="90" Margin="12,0,0,0" VerticalAlignment="Center"/>
                        <TextBlock Text="{Binding IsReconciled}" VerticalAlignment="Center" FontWeight="Bold"/>
                    </StackPanel>

                    <GroupBox Header="Items" Padding="6">
                        <DockPanel LastChildFill="True">
                            <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0,0,0,6">
                                <Button Content="Add Item" Command="{Binding AddItemCommand}" Width="90" Margin="0,0,6,0"/>
                                <Button Content="Remove Item" Command="{Binding RemoveItemCommand}" CommandParameter="{Binding SelectedItem, ElementName=ItemsGrid}" Width="110" />
                            </StackPanel>

                            <DataGrid x:Name="ItemsGrid" ItemsSource="{Binding Items}" AutoGenerateColumns="False" CanUserAddRows="False">
                                <DataGrid.Columns>
                                    <DataGridTextColumn Header="GL Line Id" Binding="{Binding GeneralLedgerLineId, Mode=TwoWay}" Width="120"/>
                                    <DataGridTextColumn Header="Amount" Binding="{Binding Amount, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                                    <DataGridCheckBoxColumn Header="Matched" Binding="{Binding IsMatched, Mode=TwoWay}" Width="120"/>
                                </DataGrid.Columns>
                            </DataGrid>
                        </DockPanel>
                    </GroupBox>
                </StackPanel>

                <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right">
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="100" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="100"/>
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

namespace GMSApp.ViewModels.Accounting
{
    public partial class AccountReconciliationViewModel : ObservableObject
    {
        private readonly IRepository<AccountReconciliation> _repo;

        public ObservableCollection<AccountReconciliation> Reconciliations { get; } = new();

        public AccountReconciliationViewModel(IRepository<AccountReconciliation> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private AccountReconciliation? selectedReconciliation;

        partial void OnSelectedReconciliationChanged(AccountReconciliation? value)
        {
            Items.Clear();
            if (value != null)
            {
                foreach (var it in value.Items)
                {
                    var ed = new ReconciliationItemEditable
                    {
                        Id = it.Id,
                        GeneralLedgerLineId = it.GeneralLedgerLineId,
                        IsMatched = it.IsMatched,
                        Amount = 0m // amount can be set by user; repository could enrich this if desired
                    };
                    ed.PropertyChanged += Item_PropertyChanged;
                    Items.Add(ed);
                }
            }

            OnPropertyChanged(nameof(LedgerBalance));
            OnPropertyChanged(nameof(IsReconciled));
            NotifyCommands();
        }

        public ObservableCollection<ReconciliationItemEditable> Items { get; } = new();

        [ObservableProperty]
        private ReconciliationItemEditable? selectedItem;

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReconciliationItemEditable.IsMatched) ||
                e.PropertyName == nameof(ReconciliationItemEditable.Amount))
            {
                OnPropertyChanged(nameof(LedgerBalance));
                OnPropertyChanged(nameof(IsReconciled));
            }
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AddItemCommand.NotifyCanExecuteChanged();
            RemoveItemCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Reconciliations.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var r in list) Reconciliations.Add(r);
                SelectedReconciliation = Reconciliations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load reconciliations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var rec = new AccountReconciliation
            {
                ReconciliationDate = DateTime.UtcNow,
                StatementBalance = 0m,
                LedgerBalance = 0m,
                IsReconciled = false
            };
            Reconciliations.Add(rec);
            SelectedReconciliation = rec;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedReconciliation == null) return;

            try
            {
                // create detached copy
                var detached = new AccountReconciliation
                {
                    Id = SelectedReconciliation.Id,
                    ChartOfAccountId = SelectedReconciliation.ChartOfAccountId,
                    ReconciliationDate = SelectedReconciliation.ReconciliationDate,
                    StatementBalance = SelectedReconciliation.StatementBalance,
                    LedgerBalance = LedgerBalance,
                    IsReconciled = IsReconciled,
                    Items = Items.Select(i => new ReconciliationItem
                    {
                        Id = i.Id,
                        GeneralLedgerLineId = i.GeneralLedgerLineId,
                        IsMatched = i.IsMatched
                    }).ToList()
                };

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedReconciliation = Reconciliations.FirstOrDefault(r => r.Id == detached.Id) ?? SelectedReconciliation;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save reconciliation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedReconciliation == null) return;

            var confirm = MessageBox.Show("Delete selected reconciliation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedReconciliation.Id == 0)
                {
                    Reconciliations.Remove(SelectedReconciliation);
                    SelectedReconciliation = Reconciliations.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedReconciliation.Id);
                    await LoadAsync();
                    SelectedReconciliation = Reconciliations.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete reconciliation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddItem()
        {
            var it = new ReconciliationItemEditable
            {
                GeneralLedgerLineId = 0,
                Amount = 0m,
                IsMatched = false
            };
            it.PropertyChanged += Item_PropertyChanged;
            Items.Add(it);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveItem(ReconciliationItemEditable? it)
        {
            if (it == null) return;
            it.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(it);
            OnPropertyChanged(nameof(LedgerBalance));
            OnPropertyChanged(nameof(IsReconciled));
        }

        public decimal LedgerBalance => Math.Round(Items.Where(i => i.IsMatched).Sum(i => i.Amount), 2);

        public bool IsReconciled
        {
            get
            {
                if (SelectedReconciliation == null) return false;
                return Math.Abs(LedgerBalance - SelectedReconciliation.StatementBalance) < 0.01m;
            }
        }

        private bool CanModify() => SelectedReconciliation != null;

        public class ReconciliationItemEditable : ObservableObject
        {
            [ObservableProperty] private int id;
            [ObservableProperty] private int generalLedgerLineId;
            [ObservableProperty] private decimal amount;
            [ObservableProperty] private bool isMatched;
        }
    }
}
<UserControl x:Class="GMSApp.Views.Accounting.AccountsPayableView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="700" d:DesignWidth="1100">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360" />
            <ColumnDefinition Width="12" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Payables}"
                          SelectedItem="{Binding SelectedPayable, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          IsReadOnly="False"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Invoice #" Binding="{Binding InvoiceNumber}" Width="*"/>
                        <DataGridTextColumn Header="Date" Binding="{Binding InvoiceDate, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="Amount" Binding="{Binding Amount, StringFormat=N2}" Width="120"/>
                        <DataGridTextColumn Header="Paid" Binding="{Binding PaidAmount, StringFormat=N2}" Width="120"/>
                        <DataGridTextColumn Header="Status" Binding="{Binding Status}" Width="120"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="VendorId:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedPayable.VendorId, Mode=TwoWay}" Width="120"/>
                    <TextBlock Text="Invoice #" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedPayable.InvoiceNumber, Mode=TwoWay}" Width="240"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Invoice Date:" Width="120" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedPayable.InvoiceDate, Mode=TwoWay}" />
                    <TextBlock Text="Due Date:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedPayable.DueDate, Mode=TwoWay}" />
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Amount:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedPayable.Amount, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                    <TextBlock Text="Paid:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedPayable.PaidAmount, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
                    <TextBlock Text="Balance:" FontWeight="Bold" Margin="0,0,8,0"/>
                    <TextBlock Text="{Binding Balance, StringFormat=N2}" FontWeight="Bold"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="100" Margin="12,0,0,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="100" Margin="6,0,0,0"/>
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

namespace GMSApp.ViewModels.Accounting
{
    public partial class AccountsPayableViewModel : ObservableObject
    {
        private readonly IRepository<AccountsPayable> _repo;

        public ObservableCollection<AccountsPayable> Payables { get; } = new();

        public AccountsPayableViewModel(IRepository<AccountsPayable> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private AccountsPayable? selectedPayable;

        partial void OnSelectedPayableChanged(AccountsPayable? value)
        {
            OnPropertyChanged(nameof(Balance));
            NotifyCommands();
        }

        private void NotifyCommands()
        {
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
                Payables.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var p in list) Payables.Add(p);
                SelectedPayable = Payables.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load AP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var ap = new AccountsPayable
            {
                InvoiceDate = DateTime.UtcNow,
                InvoiceNumber = string.Empty,
                Amount = 0m,
                PaidAmount = 0m,
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Unpaid
            };
            Payables.Add(ap);
            SelectedPayable = ap;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedPayable == null) return;

            try
            {
                var detached = new AccountsPayable
                {
                    Id = SelectedPayable.Id,
                    VendorId = SelectedPayable.VendorId,
                    InvoiceDate = SelectedPayable.InvoiceDate,
                    InvoiceNumber = SelectedPayable.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedPayable.Amount,
                    PaidAmount = SelectedPayable.PaidAmount,
                    DueDate = SelectedPayable.DueDate,
                    Status = SelectedPayable.Status
                };

                if (detached.PaidAmount <= 0) detached.Status = InvoiceStatus.Unpaid;
                else if (detached.PaidAmount < detached.Amount) detached.Status = InvoiceStatus.PartiallyPaid;
                else detached.Status = InvoiceStatus.Paid;

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedPayable = Payables.FirstOrDefault(p => p.InvoiceNumber == detached.InvoiceNumber) ?? SelectedPayable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save AP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedPayable == null) return;

            var confirm = MessageBox.Show("Delete selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedPayable.Id == 0)
                {
                    Payables.Remove(SelectedPayable);
                    SelectedPayable = Payables.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedPayable.Id);
                    await LoadAsync();
                    SelectedPayable = Payables.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete AP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public decimal Balance => SelectedPayable == null ? 0m : Math.Round(SelectedPayable.Amount - SelectedPayable.PaidAmount, 2);

        private bool CanModify() => SelectedPayable != null;
    }
}
<UserControl x:Class="GMSApp.Views.Accounting.AccountsReceivableView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="700" d:DesignWidth="1100">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360" />
            <ColumnDefinition Width="12" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Receivables}"
                          SelectedItem="{Binding SelectedReceivable, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          IsReadOnly="False"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Invoice #" Binding="{Binding InvoiceNumber}" Width="*"/>
                        <DataGridTextColumn Header="Date" Binding="{Binding InvoiceDate, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="Amount" Binding="{Binding Amount, StringFormat=N2}" Width="120"/>
                        <DataGridTextColumn Header="Received" Binding="{Binding ReceivedAmount, StringFormat=N2}" Width="120"/>
                        <DataGridTextColumn Header="Status" Binding="{Binding Status}" Width="120"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="CustomerId:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedReceivable.CustomerId, Mode=TwoWay}" Width="120"/>
                    <TextBlock Text="Invoice #" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedReceivable.InvoiceNumber, Mode=TwoWay}" Width="240"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Invoice Date:" Width="120" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedReceivable.InvoiceDate, Mode=TwoWay}" />
                    <TextBlock Text="Due Date:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedReceivable.DueDate, Mode=TwoWay}" />
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Amount:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedReceivable.Amount, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                    <TextBlock Text="Received:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedReceivable.ReceivedAmount, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
                    <TextBlock Text="Balance:" FontWeight="Bold" Margin="0,0,8,0"/>
                    <TextBlock Text="{Binding Balance, StringFormat=N2}" FontWeight="Bold"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="100" Margin="12,0,0,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="100" Margin="6,0,0,0"/>
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

namespace GMSApp.ViewModels.Accounting
{
    public partial class AccountsReceivableViewModel : ObservableObject
    {
        private readonly IRepository<AccountsReceivable> _repo;

        public ObservableCollection<AccountsReceivable> Receivables { get; } = new();

        public AccountsReceivableViewModel(IRepository<AccountsReceivable> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private AccountsReceivable? selectedReceivable;

        partial void OnSelectedReceivableChanged(AccountsReceivable? value)
        {
            OnPropertyChanged(nameof(Balance));
            NotifyCommands();
        }

        private void NotifyCommands()
        {
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
                Receivables.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var r in list) Receivables.Add(r);
                SelectedReceivable = Receivables.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load AR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var ar = new AccountsReceivable
            {
                InvoiceDate = DateTime.UtcNow,
                InvoiceNumber = string.Empty,
                Amount = 0m,
                ReceivedAmount = 0m,
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Unpaid
            };
            Receivables.Add(ar);
            SelectedReceivable = ar;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedReceivable == null) return;

            try
            {
                // create detached copy
                var detached = new AccountsReceivable
                {
                    Id = SelectedReceivable.Id,
                    CustomerId = SelectedReceivable.CustomerId,
                    InvoiceDate = SelectedReceivable.InvoiceDate,
                    InvoiceNumber = SelectedReceivable.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedReceivable.Amount,
                    ReceivedAmount = SelectedReceivable.ReceivedAmount,
                    DueDate = SelectedReceivable.DueDate,
                    Status = SelectedReceivable.Status
                };

                // update status based on amounts
                if (detached.ReceivedAmount <= 0) detached.Status = InvoiceStatus.Unpaid;
                else if (detached.ReceivedAmount < detached.Amount) detached.Status = InvoiceStatus.PartiallyPaid;
                else detached.Status = InvoiceStatus.Paid;

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedReceivable = Receivables.FirstOrDefault(r => r.InvoiceNumber == detached.InvoiceNumber) ?? SelectedReceivable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save AR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedReceivable == null) return;

            var confirm = MessageBox.Show("Delete selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedReceivable.Id == 0)
                {
                    Receivables.Remove(SelectedReceivable);
                    SelectedReceivable = Receivables.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedReceivable.Id);
                    await LoadAsync();
                    SelectedReceivable = Receivables.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete AR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public decimal Balance => SelectedReceivable == null ? 0m : Math.Round(SelectedReceivable.Amount - SelectedReceivable.ReceivedAmount, 2);

        private bool CanModify() => SelectedReceivable != null;
    }
}
<UserControl x:Class="GMSApp.Views.Accounting.GeneralLedgerView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="700" d:DesignWidth="1100">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360" />
            <ColumnDefinition Width="12" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Entries}"
                          SelectedItem="{Binding SelectedEntry, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          IsReadOnly="False"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Date" Binding="{Binding EntryDate, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="Ref" Binding="{Binding ReferenceNumber}" Width="140"/>
                        <DataGridTextColumn Header="Desc" Binding="{Binding Description}" Width="*"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

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
                    <TextBlock Text="Date:" Width="80" VerticalAlignment="Center"/>
                    <DatePicker SelectedDate="{Binding SelectedEntry.EntryDate, Mode=TwoWay}" />
                    <TextBlock Text="Reference:" Width="80" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedEntry.ReferenceNumber, Mode=TwoWay}" Width="240"/>
                </StackPanel>

                <GroupBox Grid.Row="2" Header="Lines" Padding="6">
                    <DockPanel LastChildFill="True">
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0,0,0,6">
                            <Button Content="Add Line" Command="{Binding AddLineCommand}" Width="90" Margin="0,0,6,0"/>
                            <Button Content="Remove Line" Command="{Binding RemoveLineCommand}" CommandParameter="{Binding SelectedItem, ElementName=LinesGrid}" Width="110" />
                        </StackPanel>

                        <DataGrid x:Name="LinesGrid"
                                  ItemsSource="{Binding Lines}"
                                  SelectedItem="{Binding SelectedLine, Mode=TwoWay}"
                                  AutoGenerateColumns="False"
                                  CanUserAddRows="False">
                            <DataGrid.Columns>
                                <DataGridComboBoxColumn Header="Account"
                                                        SelectedValueBinding="{Binding ChartOfAccountId, Mode=TwoWay}"
                                                        SelectedValuePath="Id"
                                                        DisplayMemberPath="AccountName"
                                                        Width="240"
                                                        ItemsSource="{Binding DataContext.ChartOfAccounts, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
                                <DataGridTextColumn Header="Debit" Binding="{Binding Debit, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="120"/>
                                <DataGridTextColumn Header="Credit" Binding="{Binding Credit, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="120"/>
                                <DataGridTextColumn Header="Amount" Binding="{Binding Amount, Mode=OneWay, StringFormat=N2}" Width="120" IsReadOnly="True"/>
                            </DataGrid.Columns>
                        </DataGrid>
                    </DockPanel>
                </GroupBox>

                <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right">
                    <StackPanel Orientation="Vertical" HorizontalAlignment="Right" Margin="0,0,12,0">
                        <TextBlock Text="Total Debits:" FontWeight="Bold"/>
                        <TextBlock Text="{Binding TotalDebits, StringFormat=N2}" HorizontalAlignment="Right"/>
                    </StackPanel>
                    <StackPanel Orientation="Vertical" HorizontalAlignment="Right" Margin="0,0,12,0">
                        <TextBlock Text="Total Credits:" FontWeight="Bold"/>
                        <TextBlock Text="{Binding TotalCredits, StringFormat=N2}" HorizontalAlignment="Right"/>
                    </StackPanel>

                    <TextBlock Text="{Binding IsBalanced, StringFormat='Balanced: {0}'}" VerticalAlignment="Center" Margin="12,0,0,0"/>
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

namespace GMSApp.ViewModels.Accounting
{
    public partial class GeneralLedgerViewModel : ObservableObject
    {
        private readonly IRepository<GeneralLedgerEntry> _repo;
        private readonly IRepository<ChartOfAccount> _coaRepo;

        public ObservableCollection<GeneralLedgerEntry> Entries { get; } = new();
        public ObservableCollection<ChartOfAccount> ChartOfAccounts { get; } = new();

        public GeneralLedgerViewModel(IRepository<GeneralLedgerEntry> repo, IRepository<ChartOfAccount> coaRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _coaRepo = coaRepo ?? throw new ArgumentNullException(nameof(coaRepo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private GeneralLedgerEntry? selectedEntry;

        partial void OnSelectedEntryChanged(GeneralLedgerEntry? value)
        {
            Lines.Clear();
            if (value != null)
            {
                foreach (var l in value.Lines)
                {
                    var ed = new EditableLine(l);
                    ed.PropertyChanged += Line_PropertyChanged;
                    Lines.Add(ed);
                }
            }

            OnPropertyChanged(nameof(TotalDebits));
            OnPropertyChanged(nameof(TotalCredits));
            NotifyCommands();
        }

        public ObservableCollection<EditableLine> Lines { get; } = new();

        [ObservableProperty]
        private EditableLine? selectedLine;

        private void Line_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditableLine.Debit) || e.PropertyName == nameof(EditableLine.Credit))
            {
                OnPropertyChanged(nameof(TotalDebits));
                OnPropertyChanged(nameof(TotalCredits));
                OnPropertyChanged(nameof(IsBalanced));
            }
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AddLineCommand.NotifyCanExecuteChanged();
            RemoveLineCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Entries.Clear();
                ChartOfAccounts.Clear();

                var list = await _repo.GetAllAsync();
                foreach (var e in list)
                    Entries.Add(e);

                var coaList = await _coaRepo.GetAllAsync();
                foreach (var c in coaList)
                    ChartOfAccounts.Add(c);

                SelectedEntry = Entries.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load general ledger entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var ent = new GeneralLedgerEntry
            {
                EntryDate = DateTime.UtcNow,
                ReferenceNumber = string.Empty,
                Description = string.Empty,
                Lines = new System.Collections.Generic.List<GeneralLedgerLine>()
            };
            Entries.Add(ent);
            SelectedEntry = ent;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedEntry == null) return;

            try
            {
                // build detached copy
                var detached = new GeneralLedgerEntry
                {
                    Id = SelectedEntry.Id,
                    EntryDate = SelectedEntry.EntryDate,
                    ReferenceNumber = SelectedEntry.ReferenceNumber?.Trim() ?? string.Empty,
                    Description = SelectedEntry.Description?.Trim() ?? string.Empty,
                    Lines = Lines.Select(l => new GeneralLedgerLine
                    {
                        Id = l.Id,
                        ChartOfAccountId = l.ChartOfAccountId,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList()
                };

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedEntry = Entries.FirstOrDefault(e => e.ReferenceNumber == detached.ReferenceNumber) ?? SelectedEntry;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save ledger entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedEntry == null) return;

            var confirm = MessageBox.Show("Delete selected ledger entry?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedEntry.Id == 0)
                {
                    Entries.Remove(SelectedEntry);
                    SelectedEntry = Entries.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedEntry.Id);
                    await LoadAsync();
                    SelectedEntry = Entries.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete ledger entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddLine()
        {
            var l = new EditableLine
            {
                ChartOfAccountId = ChartOfAccounts.FirstOrDefault()?.Id ?? 0,
                Debit = 0m,
                Credit = 0m
            };
            l.PropertyChanged += Line_PropertyChanged;
            Lines.Add(l);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveLine(EditableLine? line)
        {
            if (line == null) return;
            line.PropertyChanged -= Line_PropertyChanged;
            Lines.Remove(line);
        }

        public decimal TotalDebits => Lines.Sum(l => l.Debit);
        public decimal TotalCredits => Lines.Sum(l => l.Credit);
        public bool IsBalanced => Math.Round(TotalDebits - TotalCredits, 2) == 0m;

        private bool CanModify() => SelectedEntry != null;

        public class EditableLine : ObservableObject
        {
            [ObservableProperty]
            private int id;

            [ObservableProperty]
            private int chartOfAccountId;

            [ObservableProperty]
            private decimal debit;

            partial void OnDebitChanged(decimal value) => OnPropertyChanged(nameof(Amount));

            [ObservableProperty]
            private decimal credit;

            partial void OnCreditChanged(decimal value) => OnPropertyChanged(nameof(Amount));

            public decimal Amount => Debit - Credit;
        }
    }
}
<UserControl x:Class="GMSApp.Views.Accounting.ChartOfAccountView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="600" d:DesignWidth="900">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="340" />
            <ColumnDefinition Width="12" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Accounts}"
                          SelectedItem="{Binding SelectedAccount, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          IsReadOnly="False"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Code" Binding="{Binding AccountCode}" Width="120"/>
                        <DataGridTextColumn Header="Name" Binding="{Binding AccountName}" Width="*"/>
                        <DataGridTextColumn Header="Type" Binding="{Binding AccountType}" Width="120"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Code:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedAccount.AccountCode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="240"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Name:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedAccount.AccountName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="360"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Type:" Width="120" VerticalAlignment="Center"/>
                    <ComboBox Width="200" SelectedValue="{Binding SelectedAccount.AccountType, Mode=TwoWay}" SelectedValuePath="Content">
                        <ComboBoxItem>Asset</ComboBoxItem>
                        <ComboBoxItem>Liability</ComboBoxItem>
                        <ComboBoxItem>Equity</ComboBoxItem>
                        <ComboBoxItem>Revenue</ComboBoxItem>
                        <ComboBoxItem>Expense</ComboBoxItem>
                    </ComboBox>
                    <CheckBox Content="Active" IsChecked="{Binding SelectedAccount.IsActive, Mode=TwoWay}" Margin="12,0,0,0"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Parent Account:" Width="120" VerticalAlignment="Center"/>
                    <ComboBox ItemsSource="{Binding Accounts}" DisplayMemberPath="AccountName" SelectedValuePath="Id"
                              SelectedValue="{Binding SelectedAccount.ParentAccountId, Mode=TwoWay}" Width="360"/>
                </StackPanel>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Accounting
{
    public partial class ChartOfAccountViewModel : ObservableObject
    {
        private readonly IRepository<ChartOfAccount> _repo;

        public ObservableCollection<ChartOfAccount> Accounts { get; } = new();

        public ChartOfAccountViewModel(IRepository<ChartOfAccount> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ChartOfAccount? selectedAccount;

        partial void OnSelectedAccountChanged(ChartOfAccount? value)
        {
            // ensure parent reference exists in collection for binding
            OnPropertyChanged(nameof(Accounts));
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
                Accounts.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var a in list)
                {
                    Accounts.Add(a);
                }

                SelectedAccount = Accounts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load chart of accounts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var acc = new ChartOfAccount
            {
                AccountCode = string.Empty,
                AccountName = string.Empty,
                AccountType = AccountType.Asset,
                IsActive = true
            };
            Accounts.Add(acc);
            SelectedAccount = acc;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedAccount == null) return;

            try
            {
                // create detached copy
                var detached = new ChartOfAccount
                {
                    Id = SelectedAccount.Id,
                    AccountCode = SelectedAccount.AccountCode?.Trim() ?? string.Empty,
                    AccountName = SelectedAccount.AccountName?.Trim() ?? string.Empty,
                    AccountType = SelectedAccount.AccountType,
                    IsActive = SelectedAccount.IsActive,
                    ParentAccountId = SelectedAccount.ParentAccountId
                };

                if (detached.Id == 0)
                    await _repo.AddAsync(detached);
                else
                    await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedAccount = Accounts.FirstOrDefault(a => a.Id == detached.Id) ?? SelectedAccount;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedAccount == null) return;

            var confirm = MessageBox.Show("Delete selected account?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedAccount.Id == 0)
                {
                    Accounts.Remove(SelectedAccount);
                    SelectedAccount = Accounts.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedAccount.Id);
                    await LoadAsync();
                    SelectedAccount = Accounts.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedAccount != null;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Finance
{
    public partial class ChartOfAccountViewModel : ObservableObject
    {
        private readonly IRepository<ChartOfAccount> _repo;

        public ObservableCollection<ChartOfAccount> Accounts { get; } = new();

        public ChartOfAccountViewModel(IRepository<ChartOfAccount> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ChartOfAccount? selectedAccount;

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Accounts.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var a in list.OrderBy(x => x.AccountCode))
                {
                    Accounts.Add(a);
                }

                SelectedAccount = Accounts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load chart of accounts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var newAcc = new ChartOfAccount
            {
                AccountCode = string.Empty,
                AccountName = string.Empty,
                AccountType = AccountType.Asset,
                IsActive = true
            };

            Accounts.Add(newAcc);
            SelectedAccount = newAcc;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedAccount == null) return;

            try
            {
                var detached = new ChartOfAccount
                {
                    Id = SelectedAccount.Id,
                    AccountCode = SelectedAccount.AccountCode?.Trim() ?? string.Empty,
                    AccountName = SelectedAccount.AccountName?.Trim() ?? string.Empty,
                    AccountType = SelectedAccount.AccountType,
                    IsActive = SelectedAccount.IsActive,
                    ParentAccountId = SelectedAccount.ParentAccountId
                };

                if (detached.Id == 0)
                    await _repo.AddAsync(detached);
                else
                    await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedAccount = Accounts.FirstOrDefault(a => a.AccountCode == detached.AccountCode) ?? SelectedAccount;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedAccount == null) return;
            var ok = MessageBox.Show("Delete selected account?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedAccount.Id == 0)
                {
                    Accounts.Remove(SelectedAccount);
                    SelectedAccount = Accounts.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedAccount.Id);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedAccount != null;
    }
}

public class AccountReconciliation
{
    public int Id { get; set; }
    public int ChartOfAccountId { get; set; } // Usually Bank Accounts
    public ChartOfAccount ChartOfAccount { get; set; }

    public DateTime ReconciliationDate { get; set; }
    public decimal StatementBalance { get; set; }
    public decimal LedgerBalance { get; set; }
    public bool IsReconciled { get; set; }

    public ICollection<ReconciliationItem> Items { get; set; } = new List<ReconciliationItem>();
}

public class ReconciliationItem
{
    public int Id { get; set; }
    public int AccountReconciliationId { get; set; }
    public AccountReconciliation AccountReconciliation { get; set; }

    public int GeneralLedgerLineId { get; set; }
    public GeneralLedgerLine GeneralLedgerLine { get; set; }

    public bool IsMatched { get; set; }
}public class AccountsReceivable
{
    public int Id { get; set; }
    public int CustomerId { get; set; } // from Vendors table
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ReceivedAmount { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
}public class AccountsPayable
{
    public int Id { get; set; }
    public int VendorId { get; set; } // from Vendors table
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
}

public enum InvoiceStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overdue
}public class GeneralLedgerEntry
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // e.g., Invoice #, Payment #
    public string Description { get; set; } = string.Empty;

    public ICollection<GeneralLedgerLine> Lines { get; set; } = new List<GeneralLedgerLine>();
}

public class GeneralLedgerLine
{
    public int Id { get; set; }
    public int GeneralLedgerEntryId { get; set; }
    public GeneralLedgerEntry GeneralLedgerEntry { get; set; }

    public int ChartOfAccountId { get; set; }
    public ChartOfAccount ChartOfAccount { get; set; }

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}public class ChartOfAccount
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty; // e.g., 1010
    public string AccountName { get; set; } = string.Empty; // e.g., Cash
    public AccountType AccountType { get; set; } // Asset, Liability, Equity, Revenue, Expense
    public bool IsActive { get; set; } = true;

    // Hierarchy (Parent-Child)
    public int? ParentAccountId { get; set; }
    public ChartOfAccount? ParentAccount { get; set; }
    public ICollection<ChartOfAccount> SubAccounts { get; set; } = new List<ChartOfAccount>();
}

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
} 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Accounting
{
    // A single viewmodel that exposes collections & commands for ChartOfAccount,
    // GeneralLedgerEntry (with lines), AccountsReceivable, AccountsPayable, AccountReconciliation.
    // Repositories follow the IRepository<T> convention used elsewhere in the app.
    public partial class AccountingViewModel : ObservableObject
    {
        private readonly IRepository<ChartOfAccount> _coaRepo;
        private readonly IRepository<GeneralLedgerEntry> _glRepo;
        private readonly IRepository<AccountsReceivable> _arRepo;
        private readonly IRepository<AccountsPayable> _apRepo;
        private readonly IRepository<AccountReconciliation> _reconRepo;

        public AccountingViewModel(
            IRepository<ChartOfAccount> coaRepo,
            IRepository<GeneralLedgerEntry> glRepo,
            IRepository<AccountsReceivable> arRepo,
            IRepository<AccountsPayable> apRepo,
            IRepository<AccountReconciliation> reconRepo)
        {
            _coaRepo = coaRepo ?? throw new ArgumentNullException(nameof(coaRepo));
            _glRepo = glRepo ?? throw new ArgumentNullException(nameof(glRepo));
            _arRepo = arRepo ?? throw new ArgumentNullException(nameof(arRepo));
            _apRepo = apRepo ?? throw new ArgumentNullException(nameof(apRepo));
            _reconRepo = reconRepo ?? throw new ArgumentNullException(nameof(reconRepo));

            _ = LoadAllAsync();
        }

        #region Chart Of Accounts

        public ObservableCollection<ChartOfAccount> ChartOfAccounts { get; } = new();

        [ObservableProperty]
        private ChartOfAccount? selectedChartOfAccount;

        [RelayCommand]
        public async Task LoadCoaAsync()
        {
            try
            {
                ChartOfAccounts.Clear();
                var list = await _coaRepo.GetAllAsync();
                foreach (var a in list) ChartOfAccounts.Add(a);
                SelectedChartOfAccount = ChartOfAccounts.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load COA failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddCoaAsync()
        {
            var a = new ChartOfAccount { AccountCode = string.Empty, AccountName = string.Empty, AccountType = AccountType.Asset, IsActive = true };
            ChartOfAccounts.Add(a);
            SelectedChartOfAccount = a;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyCoa))]
        public async Task SaveCoaAsync()
        {
            if (SelectedChartOfAccount == null) return;
            try
            {
                var detached = new ChartOfAccount
                {
                    Id = SelectedChartOfAccount.Id,
                    AccountCode = SelectedChartOfAccount.AccountCode?.Trim() ?? string.Empty,
                    AccountName = SelectedChartOfAccount.AccountName?.Trim() ?? string.Empty,
                    AccountType = SelectedChartOfAccount.AccountType,
                    IsActive = SelectedChartOfAccount.IsActive,
                    ParentAccountId = SelectedChartOfAccount.ParentAccountId
                };

                if (detached.Id == 0) await _coaRepo.AddAsync(detached); else await _coaRepo.UpdateAsync(detached);
                await LoadCoaAsync();
                SelectedChartOfAccount = ChartOfAccounts.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedChartOfAccount;
            }
            catch (Exception ex) { MessageBox.Show($"Save COA failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyCoa))]
        public async Task DeleteCoaAsync()
        {
            if (SelectedChartOfAccount == null) return;
            if (SelectedChartOfAccount.Id == 0)
            {
                ChartOfAccounts.Remove(SelectedChartOfAccount);
                SelectedChartOfAccount = ChartOfAccounts.FirstOrDefault();
                return;
            }

            var ok = MessageBox.Show("Delete account?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try
            {
                await _coaRepo.DeleteAsync(SelectedChartOfAccount.Id);
                await LoadCoaAsync();
            }
            catch (Exception ex) { MessageBox.Show($"Delete COA failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanModifyCoa() => SelectedChartOfAccount != null;

        #endregion

        #region General Ledger (Entries + Lines)

        public ObservableCollection<GeneralLedgerEntry> GeneralLedgerEntries { get; } = new();

        [ObservableProperty]
        private GeneralLedgerEntry? selectedGlEntry;

        // editable lines for selected entry (two-way binding)
        public ObservableCollection<GeneralLedgerLineEditable> GlLines { get; } = new();

        partial void OnSelectedGlEntryChanged(GeneralLedgerEntry? value)
        {
            GlLines.Clear();
            if (value?.Lines != null)
            {
                foreach (var l in value.Lines)
                {
                    var ed = new GeneralLedgerLineEditable(l);
                    ed.PropertyChanged += GlLine_PropertyChanged;
                    GlLines.Add(ed);
                }
            }
            NotifyGlCommands();
        }

        private void GlLine_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeneralLedgerLineEditable.Debit) || e.PropertyName == nameof(GeneralLedgerLineEditable.Credit))
            {
                // nothing else required here, but UI can display totals bound to properties
                OnPropertyChanged(nameof(GlTotalDebit));
                OnPropertyChanged(nameof(GlTotalCredit));
            }
        }

        public decimal GlTotalDebit => GlLines.Sum(x => x.Debit);
        public decimal GlTotalCredit => GlLines.Sum(x => x.Credit);

        [RelayCommand]
        public async Task LoadGlAsync()
        {
            try
            {
                GeneralLedgerEntries.Clear();
                var list = await _glRepo.GetAllAsync();
                foreach (var e in list) GeneralLedgerEntries.Add(e);
                SelectedGlEntry = GeneralLedgerEntries.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load GL failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddGlEntryAsync()
        {
            var e = new GeneralLedgerEntry { EntryDate = DateTime.UtcNow, ReferenceNumber = string.Empty, Description = string.Empty, Lines = new System.Collections.Generic.List<GeneralLedgerLine>() };
            GeneralLedgerEntries.Add(e);
            SelectedGlEntry = e;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public async Task SaveGlEntryAsync()
        {
            if (SelectedGlEntry == null) return;
            try
            {
                // Map editable lines back to model
                SelectedGlEntry.Lines = GlLines.Select(l => new GeneralLedgerLine
                {
                    Id = l.Id,
                    ChartOfAccountId = l.ChartOfAccountId,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList();

                // Basic validation: debits must equal credits
                if (Math.Round(SelectedGlEntry.Lines.Sum(x => x.Debit), 2) != Math.Round(SelectedGlEntry.Lines.Sum(x => x.Credit), 2))
                {
                    var res = MessageBox.Show("Debits and Credits are not equal. Save anyway?", "Validation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res != MessageBoxResult.Yes) return;
                }

                var detached = new GeneralLedgerEntry
                {
                    Id = SelectedGlEntry.Id,
                    EntryDate = SelectedGlEntry.EntryDate,
                    ReferenceNumber = SelectedGlEntry.ReferenceNumber,
                    Description = SelectedGlEntry.Description,
                    Lines = SelectedGlEntry.Lines
                };

                if (detached.Id == 0) await _glRepo.AddAsync(detached); else await _glRepo.UpdateAsync(detached);
                await LoadGlAsync();
                SelectedGlEntry = GeneralLedgerEntries.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedGlEntry;
            }
            catch (Exception ex) { MessageBox.Show($"Save GL entry failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public async Task DeleteGlEntryAsync()
        {
            if (SelectedGlEntry == null) return;
            if (SelectedGlEntry.Id == 0)
            {
                GeneralLedgerEntries.Remove(SelectedGlEntry);
                SelectedGlEntry = GeneralLedgerEntries.FirstOrDefault();
                return;
            }
            var ok = MessageBox.Show("Delete GL entry?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _glRepo.DeleteAsync(SelectedGlEntry.Id); await LoadGlAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete GL failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public Task AddGlLineAsync()
        {
            var l = new GeneralLedgerLineEditable { ChartOfAccountId = 0, Debit = 0m, Credit = 0m };
            GlLines.Add(l);
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public Task RemoveGlLineAsync(GeneralLedgerLineEditable? line)
        {
            if (line == null) return Task.CompletedTask;
            GlLines.Remove(line);
            return Task.CompletedTask;
        }

        private bool CanModifyGl() => SelectedGlEntry != null;

        private void NotifyGlCommands()
        {
            AddGlLineCommand.NotifyCanExecuteChanged();
            RemoveGlLineCommand.NotifyCanExecuteChanged();
            SaveGlEntryCommand.NotifyCanExecuteChanged();
            DeleteGlEntryCommand.NotifyCanExecuteChanged();
        }

        #endregion

        #region Accounts Receivable

        public ObservableCollection<AccountsReceivable> AccountsReceivableList { get; } = new();

        [ObservableProperty]
        private AccountsReceivable? selectedAr;

        [RelayCommand]
        public async Task LoadArAsync()
        {
            try
            {
                AccountsReceivableList.Clear();
                var list = await _arRepo.GetAllAsync();
                foreach (var a in list) AccountsReceivableList.Add(a);
                SelectedAr = AccountsReceivableList.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load AR failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddArAsync()
        {
            var a = new AccountsReceivable { InvoiceDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), InvoiceNumber = string.Empty, Amount = 0m, ReceivedAmount = 0m };
            AccountsReceivableList.Add(a);
            SelectedAr = a;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyAr))]
        public async Task SaveArAsync()
        {
            if (SelectedAr == null) return;
            try
            {
                var detached = new AccountsReceivable
                {
                    Id = SelectedAr.Id,
                    CustomerId = SelectedAr.CustomerId,
                    InvoiceDate = SelectedAr.InvoiceDate,
                    InvoiceNumber = SelectedAr.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedAr.Amount,
                    ReceivedAmount = SelectedAr.ReceivedAmount,
                    DueDate = SelectedAr.DueDate,
                    Status = SelectedAr.Status
                };

                // set status
                detached.Status = detached.ReceivedAmount == 0 ? InvoiceStatus.Unpaid :
                                  detached.ReceivedAmount < detached.Amount ? InvoiceStatus.PartiallyPaid :
                                  InvoiceStatus.Paid;

                if (detached.Id == 0) await _arRepo.AddAsync(detached); else await _arRepo.UpdateAsync(detached);
                await LoadArAsync();
                SelectedAr = AccountsReceivableList.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedAr;
            }
            catch (Exception ex) { MessageBox.Show($"Save AR failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyAr))]
        public async Task DeleteArAsync()
        {
            if (SelectedAr == null) return;
            if (SelectedAr.Id == 0) { AccountsReceivableList.Remove(SelectedAr); SelectedAr = AccountsReceivableList.FirstOrDefault(); return; }
            var ok = MessageBox.Show("Delete AR record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _arRepo.DeleteAsync(SelectedAr.Id); await LoadArAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete AR failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanModifyAr() => SelectedAr != null;

        #endregion

        #region Accounts Payable

        public ObservableCollection<AccountsPayable> AccountsPayableList { get; } = new();

        [ObservableProperty]
        private AccountsPayable? selectedAp;

        [RelayCommand]
        public async Task LoadApAsync()
        {
            try
            {
                AccountsPayableList.Clear();
                var list = await _apRepo.GetAllAsync();
                foreach (var a in list) AccountsPayableList.Add(a);
                SelectedAp = AccountsPayableList.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load AP failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddApAsync()
        {
            var a = new AccountsPayable { InvoiceDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), InvoiceNumber = string.Empty, Amount = 0m, PaidAmount = 0m };
            AccountsPayableList.Add(a);
            SelectedAp = a;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyAp))]
        public async Task SaveApAsync()
        {
            if (SelectedAp == null) return;
            try
            {
                var detached = new AccountsPayable
                {
                    Id = SelectedAp.Id,
                    VendorId = SelectedAp.VendorId,
                    InvoiceDate = SelectedAp.InvoiceDate,
                    InvoiceNumber = SelectedAp.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedAp.Amount,
                    PaidAmount = SelectedAp.PaidAmount,
                    DueDate = SelectedAp.DueDate,
                    Status = SelectedAp.Status
                };

                detached.Status = detached.PaidAmount == 0 ? InvoiceStatus.Unpaid :
                                   detached.PaidAmount < detached.Amount ? InvoiceStatus.PartiallyPaid :
                                   InvoiceStatus.Paid;

                if (detached.Id == 0) await _apRepo.AddAsync(detached); else await _apRepo.UpdateAsync(detached);
                await LoadApAsync();
                SelectedAp = AccountsPayableList.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedAp;
            }
            catch (Exception ex) { MessageBox.Show($"Save AP failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyAp))]
        public async Task DeleteApAsync()
        {
            if (SelectedAp == null) return;
            if (SelectedAp.Id == 0) { AccountsPayableList.Remove(SelectedAp); SelectedAp = AccountsPayableList.FirstOrDefault(); return; }
            var ok = MessageBox.Show("Delete AP record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _apRepo.DeleteAsync(SelectedAp.Id); await LoadApAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete AP failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanModifyAp() => SelectedAp != null;

        #endregion

        #region Account Reconciliation

        public ObservableCollection<AccountReconciliation> Reconciliations { get; } = new();

        [ObservableProperty]
        private AccountReconciliation? selectedReconciliation;

        // Items presented for matching (editable boolean)
        public ObservableCollection<ReconciliationItemEditable> ReconItems { get; } = new();

        partial void OnSelectedReconciliationChanged(AccountReconciliation? value)
        {
            ReconItems.Clear();
            if (value?.Items != null)
            {
                foreach (var i in value.Items)
                {
                    var ed = new ReconciliationItemEditable(i);
                    ed.PropertyChanged += ReconItem_PropertyChanged;
                    ReconItems.Add(ed);
                }
            }
            NotifyReconCommands();
        }

        private void ReconItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReconciliationItemEditable.IsMatched))
            {
                // update reconciled indicator
                var matchedCount = ReconItems.Count(x => x.IsMatched);
                SelectedReconciliation!.IsReconciled = matchedCount > 0 && Math.Abs(SelectedReconciliation!.LedgerBalance - ReconItems.Where(x => x.IsMatched).Sum(x => x.Amount)) < 0.01m;
                OnPropertyChanged(nameof(SelectedReconciliation));
            }
        }

        [RelayCommand]
        public async Task LoadReconAsync()
        {
            try
            {
                Reconciliations.Clear();
                var list = await _reconRepo.GetAllAsync();
                foreach (var r in list) Reconciliations.Add(r);
                SelectedReconciliation = Reconciliations.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load Reconciliations failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddReconAsync()
        {
            var r = new AccountReconciliation { ReconciliationDate = DateTime.UtcNow, ChartOfAccountId = 0, StatementBalance = 0m, LedgerBalance = 0m, IsReconciled = false };
            Reconciliations.Add(r);
            SelectedReconciliation = r;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyRecon))]
        public async Task SaveReconAsync()
        {
            if (SelectedReconciliation == null) return;
            try
            {
                // Map edited items back to model
                SelectedReconciliation.Items = ReconItems.Select(i => new ReconciliationItem
                {
                    Id = i.Id,
                    GeneralLedgerLineId = i.GeneralLedgerLineId,
                    IsMatched = i.IsMatched
                }).ToList();

                var detached = new AccountReconciliation
                {
                    Id = SelectedReconciliation.Id,
                    ChartOfAccountId = SelectedReconciliation.ChartOfAccountId,
                    ReconciliationDate = SelectedReconciliation.ReconciliationDate,
                    StatementBalance = SelectedReconciliation.StatementBalance,
                    LedgerBalance = SelectedReconciliation.LedgerBalance,
                    Items = SelectedReconciliation.Items
                };

                if (detached.Id == 0) await _reconRepo.AddAsync(detached); else await _reconRepo.UpdateAsync(detached);
                await LoadReconAsync();
                SelectedReconciliation = Reconciliations.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedReconciliation;
            }
            catch (Exception ex) { MessageBox.Show($"Save reconciliation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyRecon))]
        public async Task DeleteReconAsync()
        {
            if (SelectedReconciliation == null) return;
            if (SelectedReconciliation.Id == 0) { Reconciliations.Remove(SelectedReconciliation); SelectedReconciliation = Reconciliations.FirstOrDefault(); return; }
            var ok = MessageBox.Show("Delete reconciliation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _reconRepo.DeleteAsync(SelectedReconciliation.Id); await LoadReconAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete reconciliation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // Quick helper that loads unreconciled GL lines for selected chart account into ReconItems (this requires repository support in real app)
        [RelayCommand(CanExecute = nameof(CanModifyRecon))]
        public Task LoadUnreconciledLinesAsync()
        {
            if (SelectedReconciliation == null) return Task.CompletedTask;

            // NOTE: This sample assumes _glRepo.GetAllAsync returns entries with lines and ChartOfAccountId set.
            // In a real app filter on ChartOfAccountId and whether line already in previous reconciliations.
            ReconItems.Clear();
            var lines = GeneralLedgerEntries.SelectMany(e => e.Lines ?? new System.Collections.Generic.List<GeneralLedgerLine>())
                                           .Where(l => l.ChartOfAccountId == SelectedReconciliation.ChartOfAccountId)
                                           .Select(l => new ReconciliationItemEditable
                                           {
                                               Id = 0,
                                               GeneralLedgerLineId = l.Id,
                                               Description = $"{l.Id} | D:{l.Debit:N2} C:{l.Credit:N2}",
                                               Amount = l.Debit - l.Credit,
                                               IsMatched = false
                                           });

            foreach (var r in lines) ReconItems.Add(r);

            // compute ledger balance from these lines
            SelectedReconciliation.LedgerBalance = ReconItems.Sum(x => x.Amount);
            OnPropertyChanged(nameof(SelectedReconciliation));
            return Task.CompletedTask;
        }

        private bool CanModifyRecon() => SelectedReconciliation != null;

        private void NotifyReconCommands()
        {
            LoadUnreconciledLinesCommand.NotifyCanExecuteChanged();
            SaveReconCommand.NotifyCanExecuteChanged();
            DeleteReconCommand.NotifyCanExecuteChanged();
        }

        #endregion

        #region Helpers + LoadAll

        [RelayCommand]
        public async Task LoadAllAsync()
        {
            await Task.WhenAll(LoadCoaAsync(), LoadGlAsync(), LoadArAsync(), LoadApAsync(), LoadReconAsync());
        }

        #endregion

        #region Editable helper classes

        public class GeneralLedgerLineEditable : ObservableObject
        {
            public GeneralLedgerLineEditable() { }
            public GeneralLedgerLineEditable(GeneralLedgerLine src)
            {
                Id = src.Id;
                ChartOfAccountId = src.ChartOfAccountId;
                Debit = src.Debit;
                Credit = src.Credit;
            }

            [ObservableProperty] private int id;
            [ObservableProperty] private int chartOfAccountId;
            [ObservableProperty] private decimal debit;
            partial void OnDebitChanged(decimal value) => OnPropertyChanged(nameof(Balance));
            [ObservableProperty] private decimal credit;
            partial void OnCreditChanged(decimal value) => OnPropertyChanged(nameof(Balance));
            public decimal Balance => Debit - Credit;
        }

        public class ReconciliationItemEditable : ObservableObject
        {
            public ReconciliationItemEditable() { }
            public ReconciliationItemEditable(ReconciliationItem src)
            {
                Id = src.Id;
                GeneralLedgerLineId = src.GeneralLedgerLineId;
                IsMatched = src.IsMatched;
            }

            [ObservableProperty] private int id;
            [ObservableProperty] private int generalLedgerLineId;
            [ObservableProperty] private string description = string.Empty;
            [ObservableProperty] private decimal amount;
            [ObservableProperty] private bool isMatched;
        }

        #endregion
    }
}