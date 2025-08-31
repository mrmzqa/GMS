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








*//*
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
  
}
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GMSApp.Converters
{
    
}



using GMSApp.ViewModels.Job;
using System.Windows.Controls;

namespace GMSApp.Views.Job
{
  
}*/
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GMSApp.Models
{
    public class ItemRow : ObservableObject
    {
        [Key]
        public int Id { get; set; } // EF primary key

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        [NotMapped]
        public decimal Total => Quantity * Price;

        // FK to Joborder - name follows convention so EF will wire it up automatically
        // If you prefer attribute, you can also use: [ForeignKey(nameof(Joborder))]
        public int? JoborderId { get; set; }

        // Navigation property
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
using System.Linq;

namespace GMSApp.ViewModels.Job
{
    public partial class JoborderViewModel : ObservableObject
    {
        private readonly IRepository<Joborder> _JoborderRepo;
        private readonly IFileRepository _fileRepo;

        public ObservableCollection<Joborder> Joborders { get; } = new();

        public ObservableCollection<ItemRow> Items { get; } = new();

        public decimal Total => Items.Sum(x => x.Total);

        public JoborderViewModel(IRepository<Joborder> JoborderRepo, IFileRepository fileRepo)
        {
            _JoborderRepo = JoborderRepo;
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
            if (e.PropertyName == nameof(ItemRow.Quantity) || e.PropertyName == nameof(ItemRow.Price))
                OnPropertyChanged(nameof(Total));
        }

        [ObservableProperty]
        private Joborder? selectedJoborder;

        partial void OnSelectedJoborderChanged(Joborder? value)
        {
            // sync Items from the selected joborder - clone so UI edits don't directly mutate tracked entities until save/update
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.Clear();

            if (value?.Items != null)
            {
                foreach (var it in value.Items)
                {
                    // Clone into UI items to avoid directly manipulating tracked entities
                    Items.Add(CloneForUi(it));
                }
            }

            Items.CollectionChanged += Items_CollectionChanged;

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
            var items = await _JoborderRepo.GetAllAsync();
            foreach (var item in items)
                Joborders.Add(item);

            SelectedJoborder = Joborders.Count > 0 ? Joborders[0] : null;
        }

        // Helper: clone an ItemRow for UI editing (Id = 0 means EF sees it as new)
        private static ItemRow CloneForDb(ItemRow src)
        {
            return new ItemRow
            {
                // do NOT copy Id when creating new DB entries
                Id = 0,
                Name = src.Name,
                Quantity = src.Quantity,
                Price = src.Price,
                JoborderId = null,
                Joborder = null
            };
        }

        // Clone into a separate object for UI (keeps existing Id for display if desired)
        private static ItemRow CloneForUi(ItemRow src)
        {
            return new ItemRow
            {
                Id = src.Id,
                Name = src.Name,
                Quantity = src.Quantity,
                Price = src.Price,
                JoborderId = src.JoborderId,
                Joborder = null
            };
        }

        [RelayCommand]
        public async Task AddJoborderAsync()
        {
            // Create a brand-new Joborder and clone UI items into new DB items (so EF won't try to reuse tracked entities)
            var newJoborder = new Joborder
            {
                CustomerName = SelectedJoborder?.CustomerName,
                Phonenumber = SelectedJoborder?.Phonenumber,
                VehicleNumber = SelectedJoborder?.VehicleNumber,
                Brand = SelectedJoborder?.Brand,
                Model = SelectedJoborder?.Model,
                OdoNumber = SelectedJoborder?.OdoNumber,
                F = SelectedJoborder?.F,
                FN = SelectedJoborder?.FN,
                B = SelectedJoborder?.B,
                BN = SelectedJoborder?.BN,
                LS = SelectedJoborder?.LS,
                LSN = SelectedJoborder?.LSN,
                RS = SelectedJoborder?.RS,
                RSN = SelectedJoborder?.RSN,
                Items = Items.Select(CloneForDb).ToList()
            };

            await _JoborderRepo.AddAsync(newJoborder);
            await LoadJobordersAsync();

            // set selected to the newly added joborder
            SelectedJoborder = Joborders.FirstOrDefault(j => j.Id == newJoborder.Id) ?? newJoborder;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task UpdateJoborderAsync()
        {
            if (SelectedJoborder == null) return;

            // Replace the selected joborder's items with clones of current UI Items.
            // We will:
            // - For simplicity: delete all existing items and insert new ones.
            //   (Alternatively implement a merge that updates existing items by Id.)
            SelectedJoborder.Items.Clear();
            var dbItems = Items.Select(CloneForDb).ToList();
            foreach (var it in dbItems)
                SelectedJoborder.Items.Add(it);

            await _JoborderRepo.UpdateAsync(SelectedJoborder);
            await LoadJobordersAsync();
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteJoborderAsync()
        {
            if (SelectedJoborder == null) return;
            await _JoborderRepo.DeleteAsync(SelectedJoborder.Id);
            SelectedJoborder = null;
            await LoadJobordersAsync();
        }

        [RelayCommand]
        private void AddItem()
        {
            Items.Add(new ItemRow { Name = string.Empty, Quantity = 1, Price = 0m });
        }

        [RelayCommand]
        private void RemoveItem(ItemRow item)
        {
            if (item != null) Items.Remove(item);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveCommand()
        {
            // Save current UI items into the selected joborder (either Add or Update)
            if (SelectedJoborder == null) return;

            // create db-ready clones
            var dbItems = Items.Select(CloneForDb).ToList();
            SelectedJoborder.Items.Clear();
            foreach (var it in dbItems)
                SelectedJoborder.Items.Add(it);

            if (SelectedJoborder.Id == 0)
            {
                await _JoborderRepo.AddAsync(SelectedJoborder);
            }
            else
            {
                await _JoborderRepo.UpdateAsync(SelectedJoborder);
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

