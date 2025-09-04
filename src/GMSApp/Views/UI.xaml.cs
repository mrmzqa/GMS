using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels
{
    public partial class VendorViewModel : ObservableObject
    {
        private readonly IRepository<Vendor> _repo;

        public ObservableCollection<Vendor> Vendors { get; } = new();

        public VendorViewModel(IRepository<Vendor> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private Vendor? selectedVendor;

        partial void OnSelectedVendorChanged(Vendor? value)
        {
            // Ensure Address exists for editing
            if (value != null && value.Address == null)
            {
                value.Address = new Address();
            }

            // Notify commands so Save/Delete enable state updates
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
                Vendors.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var v in list)
                {
                    // Ensure Address object present for UI binding
                    if (v.Address == null) v.Address = new Address();
                    Vendors.Add(v);
                }

                SelectedVendor = Vendors.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load vendors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create a local draft; not persisted until SaveAsync is called.
        [RelayCommand]
        public Task AddAsync()
        {
            try
            {
                var v = new Vendor
                {
                    // Id = 0 means not yet persisted
                    Name = string.Empty,
                    ContactPerson = string.Empty,
                    Phone = string.Empty,
                    Email = string.Empty,
                    CRNumber = string.Empty,
                    Address = new Address()
                };

                Vendors.Add(v);
                SelectedVendor = v;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create vendor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedVendor == null) return;

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(SelectedVendor.Name))
                {
                    MessageBox.Show("Vendor name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create a detached copy so we don't send UI-tracked entities to EF directly
                var detached = new Vendor
                {
                    Id = SelectedVendor.Id,
                    Name = SelectedVendor.Name?.Trim() ?? string.Empty,
                    ContactPerson = string.IsNullOrWhiteSpace(SelectedVendor.ContactPerson) ? null : SelectedVendor.ContactPerson?.Trim(),
                    Phone = string.IsNullOrWhiteSpace(SelectedVendor.Phone) ? null : SelectedVendor.Phone?.Trim(),
                    Email = string.IsNullOrWhiteSpace(SelectedVendor.Email) ? null : SelectedVendor.Email?.Trim(),
                    CRNumber = string.IsNullOrWhiteSpace(SelectedVendor.CRNumber) ? null : SelectedVendor.CRNumber?.Trim(),
                    AddressId = SelectedVendor.AddressId
                };

                // Copy address if present (create a new Address instance)
                if (SelectedVendor.Address != null)
                {
                    detached.Address = new Address
                    {
                        Id = SelectedVendor.Address.Id,
                        Line1 = SelectedVendor.Address.Line1?.Trim() ?? string.Empty,
                        Line2 = string.IsNullOrWhiteSpace(SelectedVendor.Address.Line2) ? null : SelectedVendor.Address.Line2?.Trim(),
                        City = SelectedVendor.Address.City?.Trim() ?? string.Empty,
                        State = string.IsNullOrWhiteSpace(SelectedVendor.Address.State) ? null : SelectedVendor.Address.State?.Trim(),
                        PostalCode = string.IsNullOrWhiteSpace(SelectedVendor.Address.PostalCode) ? null : SelectedVendor.Address.PostalCode?.Trim(),
                        Country = string.IsNullOrWhiteSpace(SelectedVendor.Address.Country) ? "Qatar" : SelectedVendor.Address.Country?.Trim()
                    };
                }

                if (detached.Id == 0)
                {
                    await _repo.AddAsync(detached);
                }
                else
                {
                    await _repo.UpdateAsync(detached);
                }

                // Reload canonical list to get assigned Ids / DB defaults
                await LoadAsync();

                // Restore selection by Id (if added, this will find the persisted item)
                SelectedVendor = Vendors.FirstOrDefault(x => x.Id == detached.Id) ??
                                 Vendors.FirstOrDefault(x => x.Name == detached.Name) ??
                                 SelectedVendor;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : string.Empty;
                MessageBox.Show($"Failed to save vendor: {ex.Message}{inner}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedVendor == null) return;

            var confirm = MessageBox.Show("Delete selected vendor?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedVendor.Id == 0)
                {
                    // Local (unsaved) vendor - remove from collection only
                    Vendors.Remove(SelectedVendor);
                    SelectedVendor = Vendors.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedVendor.Id);
                    await LoadAsync();
                    SelectedVendor = Vendors.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete vendor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedVendor != null;
    }
}<UserControl x:Class="GMSApp.Views.Vendor.VendorView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="600" d:DesignWidth="900">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="320" />
            <ColumnDefinition Width="12" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <!-- Left: Vendor list and actions -->
        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Vendors}"
                          SelectedItem="{Binding SelectedVendor, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          CanUserDeleteRows="False"
                          IsReadOnly="False"
                          Margin="0"
                          MinHeight="300">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Id" Binding="{Binding Id}" Width="60"/>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
                        <DataGridTextColumn Header="Contact" Binding="{Binding ContactPerson}" Width="120"/>
                        <DataGridTextColumn Header="Phone" Binding="{Binding Phone}" Width="110"/>
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

        <!-- Right: Vendor details + Address -->
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Vendor details -->
                <StackPanel Orientation="Vertical" Grid.Row="0" Margin="0,0,0,8">
                    <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                        <TextBlock Text="Name:" Width="100" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding SelectedVendor.Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="360"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                        <TextBlock Text="Contact Person:" Width="100" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding SelectedVendor.ContactPerson, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="360"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                        <TextBlock Text="Phone:" Width="100" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding SelectedVendor.Phone, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="200"/>
                        <TextBlock Text="Email:" Width="60" Margin="12,0,0,0" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding SelectedVendor.Email, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="220"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                        <TextBlock Text="CR Number:" Width="100" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding SelectedVendor.CRNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="200"/>
                    </StackPanel>
                </StackPanel>

                <!-- Address header -->
                <TextBlock Grid.Row="2" Text="Address" FontWeight="Bold" Margin="0,6,0,6"/>

                <!-- Address fields -->
                <Grid Grid.Row="4">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="150"/>
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0" Orientation="Vertical" Margin="0,0,12,0">
                        <TextBlock Text="Line1:" Margin="0,0,0,4"/>
                        <TextBlock Text="Line2:" Margin="0,10,0,4"/>
                        <TextBlock Text="City:" Margin="0,10,0,4"/>
                        <TextBlock Text="State:" Margin="0,10,0,4"/>
                        <TextBlock Text="Postal Code:" Margin="0,10,0,4"/>
                        <TextBlock Text="Country:" Margin="0,10,0,4"/>
                    </StackPanel>

                    <StackPanel Grid.Column="1" Orientation="Vertical">
                        <TextBox Text="{Binding SelectedVendor.Address.Line1, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
                        <TextBox Text="{Binding SelectedVendor.Address.Line2, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="0,6,0,0" />
                        <TextBox Text="{Binding SelectedVendor.Address.City, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="0,6,0,0" />
                        <TextBox Text="{Binding SelectedVendor.Address.State, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="0,6,0,0" />
                        <TextBox Text="{Binding SelectedVendor.Address.PostalCode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="0,6,0,0" />
                        <TextBox Text="{Binding SelectedVendor.Address.Country, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Margin="0,6,0,0" />
                    </StackPanel>
                </Grid>

                <!-- Footer actions -->
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" VerticalAlignment="Bottom" Margin="0,8,0,0">
                    <TextBlock VerticalAlignment="Center" Margin="0,0,12,0" Foreground="Gray" FontSize="11">
                        Tip: Click New to create draft. Click Save to persist.
                    </TextBlock>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="100" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="100" />
                </StackPanel>
            </Grid>
        </Border>
    </Grid>
</UserControl>
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMSApp.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        // Quotation Info
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime DateIssued { get; set; }
        public DateTime ValidUntil { get; set; }

        // Customer & Vehicle
        public string CustomerName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleYear { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;

        // Navigation Property
        public List<QuotationItem> Items { get; set; } = new();

        // Financial Settings
        public decimal LabourCharges { get; set; }
        public decimal Discount { get; set; }   // flat discount
        public decimal VatRate { get; set; } = 0.05m; // 5% default for Qatar

        // Computed Properties
        public decimal SubTotal => Items.Sum(i => i.Total) + LabourCharges;
        public decimal VatAmount => (SubTotal - Discount) * VatRate;
        public decimal GrandTotal => (SubTotal - Discount) + VatAmount;

        // Status
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    }

    public class QuotationItem
    {
        public int Id { get; set; }
        public int RepairQuotationId { get; set; } // FK

        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Computed
        public decimal Total => Quantity * UnitPrice;
    }

    public enum QuotationStatus
    {
        Draft,
        Sent,
        Approved,
        Rejected,
        ConvertedToJobOrder
    }
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

namespace GMSApp.ViewModels
{
    public partial class QuotationViewModel : ObservableObject
    {
        private readonly IRepository<Quotation> _repo;

        public ObservableCollection<Quotation> Quotations { get; } = new();

        // Editable items used by the UI (INotifyPropertyChanged)
        public ObservableCollection<EditableQuotationItem> Items { get; } = new();

        public QuotationViewModel(IRepository<Quotation> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private Quotation? selectedQuotation;

        partial void OnSelectedQuotationChanged(Quotation? value)
        {
            // Populate editable Items collection when selection changes
            Items.Clear();
            if (value != null)
            {
                foreach (var i in value.Items ?? Enumerable.Empty<QuotationItem>())
                {
                    var ed = new EditableQuotationItem(i);
                    ed.PropertyChanged += Item_PropertyChanged;
                    Items.Add(ed);
                }
            }

            // Ensure totals update
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(VatAmount));
            OnPropertyChanged(nameof(GrandTotal));
            NotifyCommands();
        }

        [ObservableProperty]
        private EditableQuotationItem? selectedItem;

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditableQuotationItem.Total) ||
                e.PropertyName == nameof(EditableQuotationItem.Quantity) ||
                e.PropertyName == nameof(EditableQuotationItem.UnitPrice))
            {
                // update totals bindings
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(VatAmount));
                OnPropertyChanged(nameof(GrandTotal));
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
                Quotations.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var q in list)
                {
                    // Ensure Items list not null
                    if (q.Items == null) q.Items = new System.Collections.Generic.List<QuotationItem>();
                    Quotations.Add(q);
                }

                SelectedQuotation = Quotations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load quotations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create a draft in-memory (not persisted until Save)
        [RelayCommand]
        public Task AddAsync()
        {
            try
            {
                var q = new Quotation
                {
                    QuotationNumber = $"QT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    DateIssued = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    Items = new System.Collections.Generic.List<QuotationItem>(),
                    VatRate = 0.05m
                };

                Quotations.Add(q);
                SelectedQuotation = q;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create quotation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedQuotation == null) return;

            try
            {
                // Build detached copy to avoid UI-tracked objects being sent to repo/EF
                var detached = new Quotation
                {
                    Id = SelectedQuotation.Id,
                    QuotationNumber = SelectedQuotation.QuotationNumber,
                    DateIssued = SelectedQuotation.DateIssued,
                    ValidUntil = SelectedQuotation.ValidUntil,
                    CustomerName = SelectedQuotation.CustomerName,
                    ContactNumber = SelectedQuotation.ContactNumber,
                    VehicleMake = SelectedQuotation.VehicleMake,
                    VehicleModel = SelectedQuotation.VehicleModel,
                    VehicleYear = SelectedQuotation.VehicleYear,
                    RegistrationNumber = SelectedQuotation.RegistrationNumber,
                    VIN = SelectedQuotation.VIN,
                    LabourCharges = SelectedQuotation.LabourCharges,
                    Discount = SelectedQuotation.Discount,
                    VatRate = SelectedQuotation.VatRate,
                    Status = SelectedQuotation.Status,
                    // map items from editable collection back to model list
                    Items = Items.Select(i => new QuotationItem
                    {
                        Id = i.Id,
                        RepairQuotationId = i.RepairQuotationId,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                if (detached.Id == 0)
                    await _repo.AddAsync(detached);
                else
                    await _repo.UpdateAsync(detached);

                // reload to get canonical state and IDs
                await LoadAsync();

                // restore selection by QuotationNumber (or Id)
                SelectedQuotation = Quotations.FirstOrDefault(q => q.QuotationNumber == detached.QuotationNumber) ?? SelectedQuotation;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : string.Empty;
                MessageBox.Show($"Failed to save quotation: {ex.Message}{inner}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedQuotation == null) return;

            var confirm = MessageBox.Show("Delete selected quotation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedQuotation.Id == 0)
                {
                    // local unsaved
                    Quotations.Remove(SelectedQuotation);
                    SelectedQuotation = Quotations.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedQuotation.Id);
                    SelectedQuotation = null;
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete quotation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddItem()
        {
            var item = new EditableQuotationItem
            {
                Description = string.Empty,
                Quantity = 1,
                UnitPrice = 0m
            };
            item.PropertyChanged += Item_PropertyChanged;
            Items.Add(item);
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(VatAmount));
            OnPropertyChanged(nameof(GrandTotal));
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveItem()
        {
            if (SelectedItem == null) return;
            SelectedItem.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(SelectedItem);
            SelectedItem = null;
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(VatAmount));
            OnPropertyChanged(nameof(GrandTotal));
        }

        private bool CanModify() => SelectedQuotation != null;

        // Computed totals (bound from view)
        public decimal SubTotal
        {
            get
            {
                decimal itemsTotal = Items.Sum(i => i.Total);
                return itemsTotal + (SelectedQuotation?.LabourCharges ?? 0m);
            }
        }

        public decimal VatAmount
        {
            get
            {
                var vatRate = SelectedQuotation?.VatRate ?? 0m;
                var baseAmount = SubTotal - (SelectedQuotation?.Discount ?? 0m);
                return Math.Round(baseAmount * vatRate, 2);
            }
        }

        public decimal GrandTotal
        {
            get
            {
                var grand = (SubTotal - (SelectedQuotation?.Discount ?? 0m)) + VatAmount;
                return Math.Round(grand, 2);
            }
        }

        // Editable item class used for binding to DataGrid
        public class EditableQuotationItem : ObservableObject
        {
            public EditableQuotationItem() { }

            public EditableQuotationItem(QuotationItem src)
            {
                Id = src.Id;
                RepairQuotationId = src.RepairQuotationId;
                Description = src.Description;
                Quantity = src.Quantity;
                UnitPrice = src.UnitPrice;
            }

            [ObservableProperty]
            private int id;

            [ObservableProperty]
            private int repairQuotationId;

            [ObservableProperty]
            private string description = string.Empty;

            [ObservableProperty]
            private int quantity;

            partial void OnQuantityChanged(int value)
            {
                OnPropertyChanged(nameof(Total));
            }

            [ObservableProperty]
            private decimal unitPrice;

            partial void OnUnitPriceChanged(decimal value)
            {
                OnPropertyChanged(nameof(Total));
            }

            public decimal Total => Math.Round(Quantity * UnitPrice, 2);
        }
    }
}
<UserControl x:Class="GMSApp.Views.Quotation.QuotationView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="700" d:DesignWidth="1100">

    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="340"/>
            <ColumnDefinition Width="12"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Left: list + actions -->
        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Background="WhiteSmoke">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0 0 0 8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="70" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="70" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Quotations}"
                          SelectedItem="{Binding SelectedQuotation, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          CanUserDeleteRows="False"
                          IsReadOnly="False">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Q #" Binding="{Binding QuotationNumber}" Width="*"/>
                        <DataGridTextColumn Header="Date" Binding="{Binding DateIssued, StringFormat=\{0:yyyy-MM-dd\}}" Width="120"/>
                        <DataGridTextColumn Header="Customer" Binding="{Binding CustomerName}" Width="140"/>
                        <DataGridTextColumn Header="Total" Binding="{Binding Path=Items, Converter={x:Null}, Mode=OneWay}" Width="80" />
                    </DataGrid.Columns>
                </DataGrid>
            </DockPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch" ShowsPreview="True" />

        <!-- Right: details -->
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD" BorderThickness="1" CornerRadius="4">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="8"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Header fields -->
                <StackPanel Orientation="Horizontal" Grid.Row="0">
                    <StackPanel Width="520">
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="Quotation #" Width="110" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.QuotationNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="220"/>
                        </StackPanel>

                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="Date Issued:" Width="110" VerticalAlignment="Center"/>
                            <DatePicker SelectedDate="{Binding SelectedQuotation.DateIssued, Mode=TwoWay}" />
                            <TextBlock Text="Valid Until:" Width="90" Margin="12,0,0,0" VerticalAlignment="Center"/>
                            <DatePicker SelectedDate="{Binding SelectedQuotation.ValidUntil, Mode=TwoWay}" />
                        </StackPanel>

                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="Customer:" Width="110" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.CustomerName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="360"/>
                        </StackPanel>

                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="Contact:" Width="110" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.ContactNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="200"/>
                        </StackPanel>
                    </StackPanel>

                    <StackPanel Margin="24,0,0,0" Width="300">
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="Vehicle:" Width="110" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.VehicleMake, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="120" />
                            <TextBox Text="{Binding SelectedQuotation.VehicleModel, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="120" Margin="6,0,0,0"/>
                        </StackPanel>

                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="Year:" Width="110" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.VehicleYear, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="80"/>
                            <TextBlock Text="Reg #" Width="60" Margin="12,0,0,0" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.RegistrationNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="120"/>
                        </StackPanel>

                        <StackPanel Orientation="Horizontal" Margin="0,0,0,6">
                            <TextBlock Text="VIN:" Width="110" VerticalAlignment="Center"/>
                            <TextBox Text="{Binding SelectedQuotation.VIN, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="200"/>
                        </StackPanel>
                    </StackPanel>
                </StackPanel>

                <!-- Charges -->
                <StackPanel Grid.Row="2" Orientation="Horizontal">
                    <TextBlock Text="Labour Charges:" Width="110" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedQuotation.LabourCharges, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="120"/>
                    <TextBlock Text="Discount:" Width="80" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedQuotation.Discount, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="120"/>
                    <TextBlock Text="VAT %:" Width="60" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedQuotation.VatRate, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=P0}" Width="80"/>
                </StackPanel>

                <!-- Items -->
                <GroupBox Grid.Row="4" Header="Items" Padding="6">
                    <DockPanel LastChildFill="True">
                        <StackPanel Orientation="Horizontal" DockPanel.Dock="Top" Margin="0,0,0,6">
                            <Button Content="Add Item" Command="{Binding AddItemCommand}" Width="90" Margin="0,0,6,0"/>
                            <Button Content="Remove Item" Command="{Binding RemoveItemCommand}" Width="100" Margin="0,0,6,0"/>
                        </StackPanel>

                        <DataGrid ItemsSource="{Binding Items}"
                                  SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                                  AutoGenerateColumns="False"
                                  CanUserAddRows="False">
                            <DataGrid.Columns>
                                <DataGridTextColumn Header="Description" Binding="{Binding Description, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*" />
                                <DataGridTextColumn Header="Qty" Binding="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N0}" Width="80" />
                                <DataGridTextColumn Header="Unit Price" Binding="{Binding UnitPrice, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="120" />
                                <DataGridTextColumn Header="Total" Binding="{Binding Total, Mode=OneWay, StringFormat=N2}" Width="120" IsReadOnly="True"/>
                            </DataGrid.Columns>
                        </DataGrid>
                    </DockPanel>
                </GroupBox>

                <!-- Totals footer -->
                <StackPanel Grid.Row="6" Orientation="Horizontal" HorizontalAlignment="Right">
                    <StackPanel Orientation="Vertical" HorizontalAlignment="Right">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="SubTotal:" Width="120" FontWeight="Bold"/>
                            <TextBlock Text="{Binding SubTotal, StringFormat=N2}" Width="120" TextAlignment="Right"/>
                        </StackPanel>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="VAT:" Width="120" FontWeight="Bold"/>
                            <TextBlock Text="{Binding VatAmount, StringFormat=N2}" Width="120" TextAlignment="Right"/>
                        </StackPanel>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="Discount:" Width="120" FontWeight="Bold"/>
                            <TextBlock Text="{Binding SelectedQuotation.Discount, StringFormat=N2}" Width="120" TextAlignment="Right"/>
                        </StackPanel>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="Grand Total:" Width="120" FontSize="14" FontWeight="Bold"/>
                            <TextBlock Text="{Binding GrandTotal, StringFormat=N2}" Width="120" TextAlignment="Right" FontSize="14" FontWeight="Bold"/>
                        </StackPanel>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Margin="24,0,0,0">
                        <Button Content="Save" Command="{Binding SaveCommand}" Width="100" Margin="0,0,6,0"/>
                        <Button Content="Delete" Command="{Binding DeleteCommand}" Width="100"/>
                    </StackPanel>
                </StackPanel>
            </Grid>
        </Border>
    </Grid>
</UserControl>