/*using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.job
{
    public class Joborder
    {
        [Key]
        public int Id { get; set; }


        public string? CustomerName { get; set; }


        public string? Phonenumber { get; set; }


        public string? VehicleNumber { get; set; }


        public string? Brand { get; set; }


        public string? Model { get; set; }


        public Decimal? OdoNumber { get; set; }

        public ICollection<ItemRow> Items { get; set; } = new List<ItemRow>();

        public byte[]? F { get; set; }

        public string? FN { get; set; }

        public byte[]? B { get; set; }

        public string? BN { get; set; }

        public byte[]? LS { get; set; }

        public string? LSN { get; set; }
        public byte[]? RS { get; set; }

        public string? RSN { get; set; }

        public DateTime? Created { get; set; } = DateTime.Now;


    }
}
using GMSApp.Models.job;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models;
public class ItemRow
{
    [Key]
    public int Id { get; set; } // for EF
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal Total => Quantity * Price;

    public int PurchaseOrderId { get; set; }

    [ForeignKey(nameof(Joborder.Id))]
    public int Joborderid { get; set; }

    public Joborder Joborder { get; set; }


}


*//*
using GMSApp.Data;
using GMSApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace GMSApp.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<FileItem> _fileSet;

        public FileRepository(AppDbContext context)
        {
            _context = context;
            _fileSet = context.Set<FileItem>();
        }

        public async Task<IEnumerable<FileItem>> GetAllFilesAsync() => await _fileSet.ToListAsync();

        public async Task<FileItem?> GetFileAsync(int id) => await _fileSet.FindAsync(id);

        public async Task UploadFileAsync(string filePath)
        {
            var fileData = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            var fileItem = new FileItem
            {
                FileName = fileName,
                ContentType = "application/octet-stream", // fallback type
                Data = fileData
            };

            await _fileSet.AddAsync(fileItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFileAsync(int id)
        {
            var file = await GetFileAsync(id);
            if (file != null)
            {
                _fileSet.Remove(file);
                await _context.SaveChangesAsync();
            }
        }
    }
}
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;
namespace GMSApp.Repositories;

public class GenericPdfGenerator<T> : IGenericPdfGenerator<T> where T : class
{
    public async Task GeneratePdfAsync(IEnumerable<T> items, string filePath)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text($"{typeof(T).Name} List").FontSize(18).Bold().Underline();

                        foreach (var item in items)
                        {
                            column.Item().Border(1).Padding(5).Column(row =>
                            {
                                foreach (var prop in properties)
                                {
                                    var value = prop.GetValue(item)?.ToString() ?? "null";
                                    row.Item().Text($"{prop.Name}: {value}");
                                }
                            });
                        }
                    });
            });
        }).GeneratePdf(filePath);
    }
}
using GMSApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Repositories
{
    public interface IFileRepository
    {
        Task<IEnumerable<FileItem>> GetAllFilesAsync();
        Task<FileItem?> GetFileAsync(int id);
        Task UploadFileAsync(string filePath);
        Task DeleteFileAsync(int id);
    }

}
namespace GMSApp.Repositories;
public interface IGenericPdfGenerator<T> where T : class
{
    Task GeneratePdfAsync(IEnumerable<T> items, string filePath);
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace GMSApp.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);

    }

}
using GMSApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GMSApp.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

}
<Project Sdk = "Microsoft.NET.Sdk" >

  < PropertyGroup >
    < OutputType > WinExe </ OutputType >
    < TargetFramework > net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>

  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include = "CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include = "MaterialDesignColors" Version="5.2.1" />
    <PackageReference Include = "MaterialDesignThemes" Version="5.2.1" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore" Version="9.0.7" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.7" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.7" />
    <PackageReference Include = "Microsoft.EntityFrameworkCore.Tools" Version="9.0.7">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include = "Microsoft.Extensions.DependencyInjection" Version="9.0.7" />
    <PackageReference Include = "Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include = "MimeKit" Version="4.13.0" />
    <PackageReference Include = "QuestPDF" Version="2025.7.0" />
    <PackageReference Include = "Serilog" Version="4.3.0" />
    <PackageReference Include = "Serilog.Extensions.Hosting" Version="9.0.0" />
    <PackageReference Include = "Serilog.Sinks.File" Version="7.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Folder Include = "Assets\" />
    < Folder Include="Resources\" />
    <Folder Include = "Services\" />
  </ ItemGroup >

</ Project >








*/
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models.job;
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
    public class JoborderViewModel : ObservableObject
    {
        private readonly IRepository<Joborder> _jobRepo;
        private readonly IGenericPdfGenerator<Joborder> _pdfGenerator;

        public JoborderViewModel(IRepository<Joborder> jobRepo, IGenericPdfGenerator<Joborder> pdfGenerator)
        {
            _jobRepo = jobRepo;
            _pdfGenerator = pdfGenerator;

            Joborders = new ObservableCollection<Joborder>();
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            NewCommand = new RelayCommand(New);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => SelectedJoborder != null);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedJoborder != null);
            GeneratePdfCommand = new AsyncRelayCommand(GeneratePdfAsync, () => SelectedJoborder != null);
            AddItemCommand = new RelayCommand(AddItem, () => SelectedJoborder != null);
            RemoveItemCommand = new RelayCommand<ItemRow>(RemoveItem, (r) => SelectedJoborder != null && r != null);
        }

        public ObservableCollection<Joborder> Joborders { get; }

        private Joborder? _selectedJoborder;
        public Joborder? SelectedJoborder
        {
            get => _selectedJoborder;
            set
            {
                SetProperty(ref _selectedJoborder, value);
                // Raise CanExecuteChanged on commands that depend on selection
                ((RelayCommand?)AddItemCommand)?.NotifyCanExecuteChanged();
                ((RelayCommand<ItemRow>?)RemoveItemCommand)?.NotifyCanExecuteChanged();
                ((AsyncRelayCommand?)SaveCommand)?.NotifyCanExecuteChanged();
                ((AsyncRelayCommand?)DeleteCommand)?.NotifyCanExecuteChanged();
                ((AsyncRelayCommand?)GeneratePdfCommand)?.NotifyCanExecuteChanged();
            }
        }

        public IAsyncRelayCommand LoadCommand { get; }
        public IRelayCommand NewCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IAsyncRelayCommand GeneratePdfCommand { get; }
        public IRelayCommand AddItemCommand { get; }
        public IRelayCommand<ItemRow> RemoveItemCommand { get; }

        private async Task LoadAsync()
        {
            Joborders.Clear();
            var list = await _jobRepo.GetAllAsync();
            foreach (var j in list)
                Joborders.Add(j);

            if (Joborders.Any())
                SelectedJoborder = Joborders.First();
        }

        private void New()
        {
            var jo = new Joborder
            {
                CustomerName = string.Empty,
                Phonenumber = string.Empty,
                VehicleNumber = string.Empty,
                Brand = string.Empty,
                Model = string.Empty,
                OdoNumber = 0,
                Created = DateTime.Now,
            };
            Joborders.Add(jo);
            SelectedJoborder = jo;
        }

        private async Task SaveAsync()
        {
            if (SelectedJoborder == null) return;

            try
            {
                if (SelectedJoborder.Id == 0)
                {
                    await _jobRepo.AddAsync(SelectedJoborder);
                }
                else
                {
                    await _jobRepo.UpdateAsync(SelectedJoborder);
                }

                // refresh list
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedJoborder == null) return;

            if (MessageBox.Show("Are you sure you want to delete this job order?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                await _jobRepo.DeleteAsync(SelectedJoborder.Id);
                Joborders.Remove(SelectedJoborder);
                SelectedJoborder = Joborders.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem()
        {
            if (SelectedJoborder == null) return;

            var item = new ItemRow
            {
                Name = "New item",
                Quantity = 1,
                Price = 0m,
                Joborderid = SelectedJoborder.Id
            };

            SelectedJoborder.Items.Add(item);
            // If EF tracking required, persist when Save is invoked
            // Raise collection changed by resetting SelectedJoborder (quick hack)
            var tmp = SelectedJoborder;
            SelectedJoborder = null;
            SelectedJoborder = tmp;
        }

        private void RemoveItem(ItemRow item)
        {
            if (SelectedJoborder == null || item == null) return;

            SelectedJoborder.Items.Remove(item);

            var tmp = SelectedJoborder;
            SelectedJoborder = null;
            SelectedJoborder = tmp;
        }

        private async Task GeneratePdfAsync()
        {
            if (SelectedJoborder == null) return;

            var dlg = new SaveFileDialog
            {
                Title = "Save Joborder PDF",
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Joborder_{SelectedJoborder.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                // Generate pdf for single joborder
                await _pdfGenerator.GeneratePdfAsync(new[] { SelectedJoborder }, dlg.FileName);
                MessageBox.Show("PDF generated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Optionally, open the file
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true }); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

<UserControl x:Class="GMSApp.Views.JoborderView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             MinWidth="800" MinHeight="450">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="2*" />
            <ColumnDefinition Width="3*" />
        </Grid.ColumnDefinitions>

        <!-- Left: List of joborders -->
        <StackPanel Grid.Column="0" Margin="4">
            <TextBlock Text="Job Orders" FontWeight="Bold" FontSize="14" Margin="2"/>
            <DataGrid ItemsSource="{Binding Joborders}" SelectedItem="{Binding SelectedJoborder, Mode=TwoWay}"
                      AutoGenerateColumns="False" IsReadOnly="True" Height="300" Margin="2">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Id" Binding="{Binding Id}" Width="Auto"/>
                    <DataGridTextColumn Header="Customer" Binding="{Binding CustomerName}" Width="*"/>
                    <DataGridTextColumn Header="Vehicle" Binding="{Binding VehicleNumber}" Width="*"/>
                    <DataGridTextColumn Header="Created" Binding="{Binding Created}" Width="*"/>
                </DataGrid.Columns>
            </DataGrid>

            <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" Margin="2">
                <Button Content="New" Command="{Binding NewCommand}" Margin="4"/>
                <Button Content="Load" Command="{Binding LoadCommand}" Margin="4"/>
                <Button Content="Delete" Command="{Binding DeleteCommand}" Margin="4"/>
            </StackPanel>
        </StackPanel>

        <!-- Right: Details -->
        <ScrollViewer Grid.Column="1" Margin="6">
            <StackPanel>
                <TextBlock Text="Job Order Details" FontSize="16" FontWeight="Bold" Margin="2"/>
                <StackPanel Orientation="Horizontal" Margin="2">
                    <StackPanel Width="300">
                        <TextBlock Text="Customer Name" />
                        <TextBox Text="{Binding SelectedJoborder.CustomerName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

                        <TextBlock Text="Phone Number" Margin="6,8,0,0" />
                        <TextBox Text="{Binding SelectedJoborder.Phonenumber, Mode=TwoWay}" />

                        <TextBlock Text="Vehicle Number" Margin="6,8,0,0" />
                        <TextBox Text="{Binding SelectedJoborder.VehicleNumber, Mode=TwoWay}" />

                        <TextBlock Text="Brand" Margin="6,8,0,0" />
                        <TextBox Text="{Binding SelectedJoborder.Brand, Mode=TwoWay}" />

                        <TextBlock Text="Model" Margin="6,8,0,0" />
                        <TextBox Text="{Binding SelectedJoborder.Model, Mode=TwoWay}" />

                        <TextBlock Text="Odometer" Margin="6,8,0,0" />
                        <TextBox Text="{Binding SelectedJoborder.OdoNumber, Mode=TwoWay}" />
                    </StackPanel>

                    <StackPanel Margin="16,0,0,0">
                        <TextBlock Text="Created" />
                        <TextBox Text="{Binding SelectedJoborder.Created, Mode=TwoWay}" IsReadOnly="True"/>

                        <!-- Buttons for Save / Generate PDF -->
                        <StackPanel Orientation="Horizontal" Margin="0,16,0,0">
                            <Button Content="Save" Command="{Binding SaveCommand}" Margin="2" />
                            <Button Content="Generate PDF" Command="{Binding GeneratePdfCommand}" Margin="2"/>
                        </StackPanel>
                    </StackPanel>
                </StackPanel>

                <Separator Margin="4"/>

                <TextBlock Text="Items" FontWeight="Bold" Margin="2"/>
                <DataGrid ItemsSource="{Binding SelectedJoborder.Items}" AutoGenerateColumns="False" Height="200" CanUserAddRows="False" Margin="2">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name, Mode=TwoWay}" Width="*"/>
                        <DataGridTextColumn Header="Qty" Binding="{Binding Quantity, Mode=TwoWay}" Width="60"/>
                        <DataGridTextColumn Header="Price" Binding="{Binding Price, Mode=TwoWay, StringFormat=N2}" Width="80"/>
                        <DataGridTextColumn Header="Total" Binding="{Binding Total, Mode=OneWay, StringFormat=N2}" Width="80" IsReadOnly="True"/>
                    </DataGrid.Columns>
                </DataGrid>

                <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" Margin="2">
                    <Button Content="Add Item" Command="{Binding AddItemCommand}" Margin="2"/>
                    <Button Content="Remove Selected Item" Margin="2" Click="RemoveSelectedItem_Click" />
                </StackPanel>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>


using GMSApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class JoborderView : UserControl
    {
        public JoborderView()
        {
            InitializeComponent();

            // If using DI, inject the VM. Otherwise you can create with a service locator or new.
            // For quick usage without DI:
            // DataContext = new JoborderViewModel(new Repository<Joborder>(yourDbContext), new GenericPdfGenerator<Joborder>());

            // Otherwise your App should set DataContext via DI or region injection.
        }

        private void RemoveSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GMSApp.ViewModels.JoborderViewModel vm)
            {
                // Find the DataGrid in visual tree (simple approach)
                var dg = this.FindName("PART_SelectedItemsDataGrid") as DataGrid;
                // In this XAML we didn't give a name to the DataGrid; instead search children
                // Simpler: get SelectedJoborder and Selected item from first DataGrid in visual children
                // We'll get selected from the Items DataGrid by walking the tree:
                var itemsDataGrid = FindVisualChild<System.Windows.Controls.DataGrid>(this, dgCheck: dg2 => dg2.ItemsSource == vm.SelectedJoborder?.Items);
                if (itemsDataGrid != null)
                {
                    if (itemsDataGrid.SelectedItem is GMSApp.Models.ItemRow item)
                    {
                        vm.RemoveItemCommand.Execute(item);
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent, System.Func<T, bool>? dgCheck = null) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    if (dgCheck == null || dgCheck(t))
                        return t;
                }

                var result = FindVisualChild(child, dgCheck);
                if (result != null) return result;
            }
            return null;
        }
    }
}
// Example host builder registration (Program.cs or App.xaml.cs)
using GMSApp.Data;
using GMSApp.Models.job;
using GMSApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // Configure EF DbContext - example using SQLite. Update connection string as needed.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=gmsapp.db"));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IRepository<Joborder>, Repository<Joborder>>();

        // PDF generator
        services.AddSingleton(typeof(IGenericPdfGenerator<>), typeof(GenericPdfGenerator<>));
        services.AddSingleton<IGenericPdfGenerator<Joborder>, GenericPdfGenerator<Joborder>>();

        // ViewModels
        services.AddTransient<GMSApp.ViewModels.JoborderViewModel>();

        // Views: if you want to inject VM into view constructor:
        services.AddTransient<GMSApp.Views.JoborderView>(sp =>
        {
            var vm = sp.GetRequiredService<GMSApp.ViewModels.JoborderViewModel>();
            var view = new GMSApp.Views.JoborderView();
            view.DataContext = vm;
            return view;
        });
    })
    .Build();

// Start host, etc.
await host.StartAsync();
// Resolve main window or show the view inside your shell.


using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models
{
    public class ItemRow : ObservableObject
    {
        [Key]
        public int Id { get; set; } // for EF

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(Total));
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                    OnPropertyChanged(nameof(Total));
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                    OnPropertyChanged(nameof(Total));
            }
        }

        [NotMapped]
        public decimal Total => Quantity * Price;

        public int PurchaseOrderId { get; set; }

        // Keep the same foreign key properties as before
        public int Joborderid { get; set; }

        public Models.job.Joborder? Joborder { get; set; }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models.job;
using GMSApp.Models;
using GMSApp.Repositories;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace GMSApp.ViewModels.Job
{
    public partial class JoborderViewModel : ObservableObject
    {
        private readonly IRepository<Joborder> _joborderRepo;
        private readonly IFileRepository _fileRepo;

        public ObservableCollection<Joborder> Joborders { get; } = new();

        public ObservableCollection<ItemRow> Items { get; } = new();

        public decimal Total => Items.Sum(x => x.Total);

        public JoborderViewModel(IRepository<Joborder> joborderRepo, IFileRepository fileRepo)
        {
            _joborderRepo = joborderRepo;
            _fileRepo = fileRepo;

            Items.CollectionChanged += Items_CollectionChanged;

            _ = LoadJobordersAsync();
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var ni in e.NewItems.OfType<INotifyPropertyChanged>())
                    ni.PropertyChanged += Item_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (var oi in e.OldItems.OfType<INotifyPropertyChanged>())
                    oi.PropertyChanged -= Item_PropertyChanged;
            }

            OnPropertyChanged(nameof(Total));
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemRow.Quantity) || e.PropertyName == nameof(ItemRow.Price) || e.PropertyName == nameof(ItemRow.Name))
                OnPropertyChanged(nameof(Total));
        }

        [ObservableProperty]
        private Joborder? selectedJoborder;

        partial void OnSelectedJoborderChanged(Joborder? value)
        {
            // When the SelectedJoborder changes, sync Items collection from the model
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.Clear();
            if (value?.Items != null)
            {
                foreach (var it in value.Items)
                    Items.Add(it);
            }
            Items.CollectionChanged += Items_CollectionChanged;

            // Subscribe individual item PropertyChanged to recalc totals (ItemRow implements ObservableObject)
            foreach (var item in Items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged -= Item_PropertyChanged;
            foreach (var item in Items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged += Item_PropertyChanged;

            UpdateJoborderCommand.NotifyCanExecuteChanged();
            DeleteJoborderCommand.NotifyCanExecuteChanged();
            FrontFileCommand.NotifyCanExecuteChanged();
            BackFileCommand.NotifyCanExecuteChanged();
            LeftFileCommand.NotifyCanExecuteChanged();
            RightFileCommand.NotifyCanExecuteChanged();

            OnPropertyChanged(nameof(Total));
        }

        [RelayCommand]
        public async Task LoadJobordersAsync()
        {
            Joborders.Clear();
            var items = await _joborderRepo.GetAllAsync();
            foreach (var item in items)
                Joborders.Add(item);

            SelectedJoborder = Joborders.Count > 0 ? Joborders[0] : null;
        }

        [RelayCommand]
        public async Task AddJoborderAsync()
        {
            var newJoborder = new Joborder
            {
                CustomerName = SelectedJoborder?.CustomerName,
                Phonenumber = SelectedJoborder?.Phonenumber,
                VehicleNumber = SelectedJoborder?.VehicleNumber,
                Brand = SelectedJoborder?.Brand,
                Model = SelectedJoborder?.Model,
                OdoNumber = SelectedJoborder?.OdoNumber,
                Items = Items.ToList()
            };

            await _joborderRepo.AddAsync(newJoborder);
            await LoadJobordersAsync();
            SelectedJoborder = newJoborder;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            // ensure Items in the model are updated
            SelectedJoborder.Items = Items.ToList();

            await _joborderRepo.UpdateAsync(SelectedJoborder);
            await LoadJobordersAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;
            await _joborderRepo.DeleteAsync(SelectedJoborder.Id);
            SelectedJoborder = null;
            await LoadJobordersAsync();
        }

        [RelayCommand]
        private void AddItem()
        {
            var it = new ItemRow { Name = string.Empty, Quantity = 1, Price = 0m };
            Items.Add(it);
        }

        [RelayCommand]
        private void RemoveItem(ItemRow item)
        {
            if (item == null) return;
            Items.Remove(item);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveCommand()
        {
            if (SelectedJoborder == null) return;

            // Persist items into the model and update repository
            SelectedJoborder.Items = Items.ToList();
            if (SelectedJoborder.Id == 0)
            {
                await _joborderRepo.AddAsync(SelectedJoborder);
            }
            else
            {
                await _joborderRepo.UpdateAsync(SelectedJoborder);
            }

            await LoadJobordersAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void FrontFile()
        {
            if (SelectedJoborder == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Front Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.F = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.FN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void BackFile()
        {
            if (SelectedJoborder == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Back Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.B = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.BN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void LeftFile()
        {
            if (SelectedJoborder == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Left Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.LS = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.LSN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public void RightFile()
        {
            if (SelectedJoborder == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Select Right Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedJoborder.RS = File.ReadAllBytes(dialog.FileName);
                SelectedJoborder.RSN = Path.GetFileName(dialog.FileName);
                OnPropertyChanged(nameof(SelectedJoborder));
            }
        }

        private bool CanModify() => SelectedJoborder != null;
    }
}
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GMSApp.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(bytes);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}


<UserControl x:Class="GMSApp.Views.Job.JobOrder"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:viewmodels="clr-namespace:GMSApp.ViewModels.Job"
             xmlns:converters="clr-namespace:GMSApp.Converters"
             mc:Ignorable="d" d:DesignHeight="700" d:DesignWidth="900">

    <UserControl.Resources>
        <converters:ByteArrayToImageConverter x:Key="ByteArrayToImageConverter"/>
    </UserControl.Resources>

    <Grid Margin="0">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <StackPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,0,10">
            <Label Content="Search:" VerticalAlignment="Center" Margin="0,0,5,0"/>
            <TextBox Width="250" x:Name="SearchBox" />
            <Button Content="Clear" Margin="5,0,0,0" />
            <Button Content="Add" Margin="20,0,0,0" Width="100" Command="{Binding AddJoborderCommand}" />
        </StackPanel>

        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="2*"/>
                <ColumnDefinition Width="3*"/>
            </Grid.ColumnDefinitions>

            <DataGrid Grid.Column="0"
                      ItemsSource="{Binding Joborders}"
                      SelectedItem="{Binding SelectedJoborder, Mode=TwoWay}"
                      AutoGenerateColumns="True"
                      CanUserAddRows="False"
                      CanUserDeleteRows="False"
                      IsReadOnly="True"
                      SelectionMode="Single"
                      Margin="4" />

            <StackPanel Grid.Column="1" Margin="4">
                <DataGrid ItemsSource="{Binding Items}" AutoGenerateColumns="False" CanUserAddRows="False" Height="200">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                        <DataGridTextColumn Header="Quantity" Binding="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="100"/>
                        <DataGridTextColumn Header="Price" Binding="{Binding Price, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="100"/>
                        <DataGridTextColumn Header="Total" Binding="{Binding Total, StringFormat=N2}" IsReadOnly="True" Width="100"/>
                        <DataGridTemplateColumn Header="Action" Width="80">
                            <DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <Button Content="Delete"
                                            Command="{Binding DataContext.RemoveItemCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                            CommandParameter="{Binding}" />
                                </DataTemplate>
                            </DataGridTemplateColumn.CellTemplate>
                        </DataGridTemplateColumn>
                    </DataGrid.Columns>
                </DataGrid>

                <StackPanel Orientation="Horizontal" Margin="0,8,0,0">
                    <Button Content="Add Item" Width="90" Command="{Binding AddItemCommand}" />
                    <Button Content="Save Items" Width="90" Margin="8,0,0,0" Command="{Binding SaveCommand}" />
                    <TextBlock Text="Total:" VerticalAlignment="Center" Margin="16,0,0,0"/>
                    <TextBlock Text="{Binding Total, StringFormat=N2}" VerticalAlignment="Center" Margin="4,0,0,0" FontWeight="Bold"/>
                </StackPanel>

                <Border BorderBrush="Gray" BorderThickness="1" Padding="10" Margin="0,10,0,0" Background="#FFF9F9F9">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="130"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="130"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <Label Content="Customer Name:" Grid.Column="0" VerticalAlignment="Center" Grid.Row="0"/>
                        <TextBox Text="{Binding SelectedJoborder.CustomerName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Grid.Column="1" Margin="5" Grid.Row="0"/>

                        <Label Content="Phonenumber" Grid.Column="2" VerticalAlignment="Center" Grid.Row="0"/>
                        <TextBox Text="{Binding SelectedJoborder.Phonenumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Grid.Column="3" Margin="5" Grid.Row="0"/>

                        <Label Content="VehicleNumber:" Grid.Column="0" VerticalAlignment="Center" Grid.Row="1"/>
                        <TextBox Text="{Binding SelectedJoborder.VehicleNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Grid.Column="1" Margin="5" Grid.Row="1"/>

                        <Label Content="Brand:" Grid.Column="2" VerticalAlignment="Center" Grid.Row="1"/>
                        <TextBox Text="{Binding SelectedJoborder.Brand, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Grid.Column="3" Margin="5" Grid.Row="1"/>

                        <Label Content="Model:" Grid.Column="0" VerticalAlignment="Center" Grid.Row="2"/>
                        <TextBox Text="{Binding SelectedJoborder.Model, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Grid.Column="1" Margin="5" Grid.Row="2"/>

                        <Label Content="Odo:" Grid.Column="2" VerticalAlignment="Center" Grid.Row="2"/>
                        <TextBox Text="{Binding SelectedJoborder.OdoNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Grid.Column="3" Margin="5" Grid.Row="2"/>

                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Grid.Column="3" Grid.Row="3" Margin="0,10,0,0">
                            <Button Content="Update" Width="100" Margin="0,0,5,0" Command="{Binding UpdateJoborderCommand}" />
                            <Button Content="Delete" Width="100" Command="{Binding DeleteJoborderCommand}" />
                        </StackPanel>
                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Grid.Column="2" Grid.Row="3" Margin="0,10,0,0">
                            <Button Content="Payment" Width="50" Margin="5,0"/>
                            <Button Content="Print" Width="50" Margin="5,0"/>
                        </StackPanel>
                    </Grid>
                </Border>

                <StackPanel Orientation="Horizontal" Margin="0,10,0,0" HorizontalAlignment="Left">
                    <Image Width="64" Height="64" Margin="4" Source="{Binding SelectedJoborder.F, Converter={StaticResource ByteArrayToImageConverter}}" />
                    <Image Width="64" Height="64" Margin="4" Source="{Binding SelectedJoborder.B, Converter={StaticResource ByteArrayToImageConverter}}" />
                    <Image Width="64" Height="64" Margin="4" Source="{Binding SelectedJoborder.LS, Converter={StaticResource ByteArrayToImageConverter}}" />
                    <Image Width="64" Height="64" Margin="4" Source="{Binding SelectedJoborder.RS, Converter={StaticResource ByteArrayToImageConverter}}" />
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,8,0,0" HorizontalAlignment="Left">
                    <Button Content="F" Width="32" Command="{Binding FrontFileCommand}" />
                    <Button Content="B" Width="32" Margin="4,0,0,0" Command="{Binding BackFileCommand}" />
                    <Button Content="L" Width="32" Margin="4,0,0,0" Command="{Binding LeftFileCommand}" />
                    <Button Content="R" Width="32" Margin="4,0,0,0" Command="{Binding RightFileCommand}" />
                </StackPanel>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>

using GMSApp.ViewModels.Job;
using System.Windows.Controls;

namespace GMSApp.Views.Job
{
    public partial class JobOrder : UserControl
    {
        // Parameterless ctor for designer support
        public JobOrder()
        {
            InitializeComponent();
        }

        // Use this constructor when composing from DI (e.g. in a window or a view locator)
        public JobOrder(JoborderViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}