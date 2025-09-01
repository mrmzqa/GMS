
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels
{
    public partial class PurchaseOrderViewModel : ObservableObject
    {
        private readonly IRepository<PurchaseOrder> _repo;
        private readonly IFileRepository _fileRepo;
        private readonly IGenericPdfGenerator<PurchaseOrder> _pdfGenerator;

        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; } = new();
        public ObservableCollection<PurchaseOrderLineViewModel> LineItems { get; } = new();

        public PurchaseOrderViewModel(IRepository<PurchaseOrder> repo,
                                      IFileRepository fileRepo,
                                      IGenericPdfGenerator<PurchaseOrder> pdfGenerator)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private PurchaseOrder? selectedPurchaseOrder;

        partial void OnSelectedPurchaseOrderChanged(PurchaseOrder? value)
        {
            PopulateLineItemsFromSelected();
            NotifyCommands();
        }

        private void NotifyCommands()
        {
            AddCommand.NotifyCanExecuteChanged();
            UpdateCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            PrintCommand.NotifyCanExecuteChanged();
            UploadFileCommand.NotifyCanExecuteChanged();
        }

        private void PopulateLineItemsFromSelected()
        {
            LineItems.Clear();

            if (SelectedPurchaseOrder == null) return;

            // Create editable viewmodels for each line to support immediate notifications and total updates.
            if (SelectedPurchaseOrder.Lines != null)
            {
                foreach (var line in SelectedPurchaseOrder.Lines)
                {
                    var vm = new PurchaseOrderLineViewModel(line);
                    vm.PropertyChanged += Line_PropertyChanged;
                    LineItems.Add(vm);
                }
            }

            RecalculateTotalsFromLineItems();
        }

        private void Line_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PurchaseOrderLineViewModel.Quantity) ||
                e.PropertyName == nameof(PurchaseOrderLineViewModel.UnitPrice) ||
                e.PropertyName == nameof(PurchaseOrderLineViewModel.Description))
            {
                RecalculateTotalsFromLineItems();
            }
        }

        private void RecalculateTotalsFromLineItems()
        {
            if (SelectedPurchaseOrder == null) return;

            // Update LineTotal for each line vm and compute SubTotal/Total
            decimal subtotal = 0m;
            foreach (var l in LineItems)
            {
                l.CalculateLineTotal();
                subtotal += l.LineTotal;
            }

            SelectedPurchaseOrder.SubTotal = Math.Round(subtotal, 2);
            SelectedPurchaseOrder.Total = Math.Round(SelectedPurchaseOrder.SubTotal - SelectedPurchaseOrder.Discount + SelectedPurchaseOrder.Tax, 2);

            OnPropertyChanged(nameof(SelectedPurchaseOrder));
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            PurchaseOrders.Clear();
            try
            {
                var list = await _repo.GetAllAsync();
                foreach (var p in list) PurchaseOrders.Add(p);

                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load purchase orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task AddAsync()
        {
            try
            {
                // Create an empty PurchaseOrder and persist it, then reload.
                var po = new PurchaseOrder
                {
                    PONumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Date = DateTime.UtcNow
                };

                // Add any lines currently in the UI (optional) - attach current LineItems if present
                foreach (var li in LineItems)
                {
                    po.Lines.Add(li.ToModel());
                }

                po.RecalculateTotals();

                await _repo.AddAsync(po);
                await LoadAsync();

                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.PONumber == po.PONumber) ?? SelectedPurchaseOrder;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save (alias for update/add based on Id)
        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                // Map LineItems back to model objects and attach to SelectedPurchaseOrder
                SelectedPurchaseOrder.Lines = LineItems.Select(li => li.ToModel()).ToList();

                SelectedPurchaseOrder.RecalculateTotals();

                if (SelectedPurchaseOrder.Id == 0)
                    await _repo.AddAsync(SelectedPurchaseOrder);
                else
                    await _repo.UpdateAsync(SelectedPurchaseOrder);

                await LoadAsync();
                // re-select by Order number or Id
                SelectedPurchaseOrder = PurchaseOrders.FirstOrDefault(p => p.PONumber == SelectedPurchaseOrder.PONumber) ?? SelectedPurchaseOrder;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateAsync()
        {
            await SaveAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            var res = MessageBox.Show("Delete selected purchase order?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            try
            {
                await _repo.DeleteAsync(SelectedPurchaseOrder.Id);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete purchase order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddLine()
        {
            var lineVm = new PurchaseOrderLineViewModel()
            {
                Description = string.Empty,
                Quantity = 1,
                UnitPrice = 0m
            };
            lineVm.PropertyChanged += Line_PropertyChanged;
            LineItems.Add(lineVm);
            RecalculateTotalsFromLineItems();
        }

        [RelayCommand]
        private void RemoveLine(PurchaseOrderLineViewModel? line)
        {
            if (line == null) return;
            line.PropertyChanged -= Line_PropertyChanged;
            LineItems.Remove(line);
            RecalculateTotalsFromLineItems();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private async Task UploadFileAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            var dlg = new OpenFileDialog { Title = "Select file to upload", Filter = "All files|*.*" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                await _fileRepo.UploadFileAsync(dlg.FileName);
                MessageBox.Show("File uploaded successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"File upload failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private async Task PrintAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                // Build a fresh model to pass to PDF generator (avoid EF-tracking issues)
                var model = new PurchaseOrder
                {
                    Id = SelectedPurchaseOrder.Id,
                    PONumber = SelectedPurchaseOrder.PONumber,
                    Date = SelectedPurchaseOrder.Date,
                    VendorId = SelectedPurchaseOrder.VendorId,
                    Vendor = SelectedPurchaseOrder.Vendor,
                    Notes = SelectedPurchaseOrder.Notes,
                    Discount = SelectedPurchaseOrder.Discount,
                    Tax = SelectedPurchaseOrder.Tax,
                    SubTotal = SelectedPurchaseOrder.SubTotal,
                    Total = SelectedPurchaseOrder.Total,
                    Currency = SelectedPurchaseOrder.Currency,
                    Status = SelectedPurchaseOrder.Status,
                    PaymentMethod = SelectedPurchaseOrder.PaymentMethod,
                    BankName = SelectedPurchaseOrder.BankName,
                    IBAN = SelectedPurchaseOrder.IBAN,
                    ExpectedDeliveryDate = SelectedPurchaseOrder.ExpectedDeliveryDate,
                    DeliveryLocation = SelectedPurchaseOrder.DeliveryLocation,
                    CreatedAt = SelectedPurchaseOrder.CreatedAt,
                    CreatedBy = SelectedPurchaseOrder.CreatedBy,
                    UpdatedAt = SelectedPurchaseOrder.UpdatedAt,
                    UpdatedBy = SelectedPurchaseOrder.UpdatedBy
                };

                model.Lines = LineItems.Select(li => li.ToModel()).ToList();
                model.RecalculateTotals();

                // Ask for save location
                var dlg = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = $"{model.PONumber}.pdf" };
                if (dlg.ShowDialog() != true) return;
                var path = dlg.FileName;

                await _pdfGenerator.GeneratePdfAsync(new[] { model }, path);

                // Try to open
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch
                {
                    // ignore if system can't open
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedPurchaseOrder != null;

        #region Line viewmodel (inner class)
        // Small VM wrapper for PurchaseOrderLine that supports INotifyPropertyChanged and computed LineTotal
        public class PurchaseOrderLineViewModel : ObservableObject
        {
            public PurchaseOrderLineViewModel() { }

            public PurchaseOrderLineViewModel(PurchaseOrderLine model)
            {
                if (model == null) return;
                Id = model.Id;
                Description = model.Description;
                PartNumber = model.PartNumber;
                UnitPrice = model.UnitPrice;
                Quantity = model.Quantity;
                Unit = model.Unit;
                Notes = model.Notes;
                QuantityDelivered = model.QuantityDelivered;
            }

            public int Id { get; set; }

            private string _description = string.Empty;
            public string Description
            {
                get => _description;
                set => SetProperty(ref _description, value);
            }

            private string? _partNumber;
            public string? PartNumber
            {
                get => _partNumber;
                set => SetProperty(ref _partNumber, value);
            }

            private decimal _unitPrice;
            public decimal UnitPrice
            {
                get => _unitPrice;
                set
                {
                    if (SetProperty(ref _unitPrice, value))
                        OnPropertyChanged(nameof(LineTotal));
                }
            }

            private decimal _quantity;
            public decimal Quantity
            {
                get => _quantity;
                set
                {
                    if (SetProperty(ref _quantity, value))
                        OnPropertyChanged(nameof(LineTotal));
                }
            }

            private string? _unit = "pc";
            public string? Unit
            {
                get => _unit;
                set => SetProperty(ref _unit, value);
            }

            private string? _notes;
            public string? Notes
            {
                get => _notes;
                set => SetProperty(ref _notes, value);
            }

            private decimal _quantityDelivered;
            public decimal QuantityDelivered
            {
                get => _quantityDelivered;
                set => SetProperty(ref _quantityDelivered, value);
            }

            // Computed
            public decimal LineTotal => Math.Round(UnitPrice * Quantity, 2);

            // Call to force recalculation and notify
            public void CalculateLineTotal()
            {
                OnPropertyChanged(nameof(LineTotal));
            }

            public PurchaseOrderLine ToModel()
            {
                return new PurchaseOrderLine
                {
                    Id = this.Id,
                    Description = this.Description ?? string.Empty,
                    PartNumber = this.PartNumber,
                    UnitPrice = this.UnitPrice,
                    Quantity = this.Quantity,
                    LineTotal = Math.Round(this.UnitPrice * this.Quantity, 2),
                    Unit = this.Unit,
                    Notes = this.Notes,
                    QuantityDelivered = this.QuantityDelivered
                };
            }
        }
        #endregion
    }
}

<UserControl x:Class="GMSApp.Views.PurchaseOrderView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:GMSApp.ViewModels"
             mc:Ignorable="d" d:DesignHeight="700" d:DesignWidth="1000">

    <UserControl.DataContext>
        <vm:PurchaseOrderViewModel />
    </UserControl.DataContext>

    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="2*" />
            <ColumnDefinition Width="3*" />
        </Grid.ColumnDefinitions>

        <!-- Left: PurchaseOrders list -->
        <StackPanel Grid.Column="0" Margin="4">
            <TextBlock Text="Purchase Orders" FontSize="16" FontWeight="Bold" Margin="0,0,0,8" />
            <DataGrid ItemsSource="{Binding PurchaseOrders}" SelectedItem="{Binding SelectedPurchaseOrder, Mode=TwoWay}"
                      AutoGenerateColumns="False" CanUserAddRows="False" Height="500">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="PO Number" Binding="{Binding PONumber}" Width="*"/>
                    <DataGridTextColumn Header="Date" Binding="{Binding Date, StringFormat=yyyy-MM-dd}" Width="120"/>
                    <DataGridTextColumn Header="Vendor" Binding="{Binding Vendor.Name}" Width="*"/>
                    <DataGridTextColumn Header="Total" Binding="{Binding Total, StringFormat=N2}" Width="100"/>
                    <DataGridTextColumn Header="Status" Binding="{Binding Status}" Width="120"/>
                </DataGrid.Columns>
            </DataGrid>

            <StackPanel Orientation="Horizontal" Margin="0,8,0,0" HorizontalAlignment="Left">
                <Button Content="Reload" Width="80" Margin="0,0,8,0" Command="{Binding LoadCommand}" />
                <Button Content="Add" Width="80" Margin="0,0,8,0" Command="{Binding AddCommand}" />
                <Button Content="Delete" Width="80" Command="{Binding DeleteCommand}" />
            </StackPanel>
        </StackPanel>

        <!-- Right: Details -->
        <StackPanel Grid.Column="1" Margin="8">
            <TextBlock Text="Purchase Order Details" FontSize="16" FontWeight="Bold" Margin="0,0,0,8" />
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="140"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="140"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <TextBlock Grid.Row="0" Grid.Column="0" Text="PO Number:" VerticalAlignment="Center" Margin="4"/>
                <TextBox Grid.Row="0" Grid.Column="1" Text="{Binding SelectedPurchaseOrder.PONumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="4"/>

                <TextBlock Grid.Row="0" Grid.Column="2" Text="Date:" VerticalAlignment="Center" Margin="4"/>
                <DatePicker Grid.Row="0" Grid.Column="3" SelectedDate="{Binding SelectedPurchaseOrder.Date, Mode=TwoWay}" Margin="4"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="Vendor:" VerticalAlignment="Center" Margin="4"/>
                <TextBox Grid.Row="1" Grid.Column="1" Text="{Binding SelectedPurchaseOrder.Vendor.Name, Mode=TwoWay}" Margin="4" />

                <TextBlock Grid.Row="1" Grid.Column="2" Text="Currency:" VerticalAlignment="Center" Margin="4"/>
                <TextBox Grid.Row="1" Grid.Column="3" Text="{Binding SelectedPurchaseOrder.Currency, Mode=TwoWay}" Margin="4"/>

                <TextBlock Grid.Row="2" Grid.Column="0" Text="Discount:" VerticalAlignment="Center" Margin="4"/>
                <TextBox Grid.Row="2" Grid.Column="1" Text="{Binding SelectedPurchaseOrder.Discount, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="4"/>

                <TextBlock Grid.Row="2" Grid.Column="2" Text="Tax:" VerticalAlignment="Center" Margin="4"/>
                <TextBox Grid.Row="2" Grid.Column="3" Text="{Binding SelectedPurchaseOrder.Tax, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="4"/>

                <TextBlock Grid.Row="3" Grid.Column="0" Text="Delivery:" VerticalAlignment="Center" Margin="4"/>
                <TextBox Grid.Row="3" Grid.Column="1" Text="{Binding SelectedPurchaseOrder.DeliveryLocation, Mode=TwoWay}" Margin="4"/>

                <TextBlock Grid.Row="3" Grid.Column="2" Text="Expected:" VerticalAlignment="Center" Margin="4"/>
                <DatePicker Grid.Row="3" Grid.Column="3" SelectedDate="{Binding SelectedPurchaseOrder.ExpectedDeliveryDate, Mode=TwoWay}" Margin="4"/>

                <TextBlock Grid.Row="4" Grid.Column="0" Text="Notes:" VerticalAlignment="Top" Margin="4"/>
                <TextBox Grid.Row="4" Grid.Column="1" Grid.ColumnSpan="3" Text="{Binding SelectedPurchaseOrder.Notes, Mode=TwoWay}" AcceptsReturn="True" Height="60" Margin="4"/>
            </Grid>

            <!-- Lines -->
            <GroupBox Header="Lines" Margin="0,8,0,0">
                <DockPanel>
                    <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="4">
                        <Button Content="Add Line" Width="90" Command="{Binding AddLineCommand}" Margin="0,0,8,0"/>
                        <Button Content="Remove Line" Width="110" Command="{Binding RemoveLineCommand}" CommandParameter="{Binding SelectedItem, ElementName=LinesGrid}" />
                        <Button Content="Save" Width="90" Margin="12,0,0,0" Command="{Binding SaveCommand}" />
                        <Button Content="Print" Width="90" Margin="8,0,0,0" Command="{Binding PrintCommand}" />
                        <Button Content="Upload File" Width="100" Margin="8,0,0,0" Command="{Binding UploadFileCommand}" />
                    </StackPanel>

                    <DataGrid x:Name="LinesGrid" ItemsSource="{Binding LineItems}" AutoGenerateColumns="False" CanUserAddRows="False" Margin="4" Height="220">
                        <DataGrid.Columns>
                            <DataGridTextColumn Header="Description" Binding="{Binding Description, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                            <DataGridTextColumn Header="Part No" Binding="{Binding PartNumber, Mode=TwoWay}" Width="120"/>
                            <DataGridTextColumn Header="Qty" Binding="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="80"/>
                            <DataGridTextColumn Header="Unit Price" Binding="{Binding UnitPrice, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="100"/>
                            <DataGridTextColumn Header="Line Total" Binding="{Binding LineTotal, Mode=OneWay, StringFormat=N2}" IsReadOnly="True" Width="110"/>
                        </DataGrid.Columns>
                    </DataGrid>
                </DockPanel>
            </GroupBox>

            <!-- Totals -->
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
                <TextBlock Text="SubTotal:" Margin="0,0,6,0" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding SelectedPurchaseOrder.SubTotal, StringFormat=N2}" FontWeight="Bold" Margin="0,0,20,0" VerticalAlignment="Center"/>
                <TextBlock Text="Total:" Margin="0,0,6,0" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding SelectedPurchaseOrder.Total, StringFormat=N2}" FontWeight="Bold" VerticalAlignment="Center"/>
            </StackPanel>

            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
                <Button Content="Reload" Width="90" Command="{Binding LoadCommand}" Margin="0,0,8,0"/>
                <Button Content="Update" Width="90" Command="{Binding UpdateCommand}" />
            </StackPanel>
        </StackPanel>
    </Grid>
</UserControl>


using GMSApp.ViewModels;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class PurchaseOrderView : UserControl
    {
        public PurchaseOrderView()
        {
            InitializeComponent();
        }

        // Optional constructor for DI
        public PurchaseOrderView(PurchaseOrderViewModel vm) : this()
        {
            DataContext = vm;
        }
    }
}


