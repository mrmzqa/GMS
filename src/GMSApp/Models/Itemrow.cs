
<Window x:Class="YourNamespace.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:local="clr-namespace:YourNamespace"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:YourNamespace"
        mc:Ignorable="d"
        Title="Purchase Order" Height="500" Width="700">

    <Window.DataContext>
        <vm:PurchaseOrderViewModel />
    </Window.DataContext>

    <StackPanel Margin="10" VerticalAlignment="Top">
        <TextBlock Text="Order Number" />
        <TextBox Text="{Binding OrderNumber}" Width="200" />

        <TextBlock Text="Order Date" Margin="0,10,0,0"/>
        <DatePicker SelectedDate="{Binding Date}" Width="200" />

        <TextBlock Text="Items" Margin="0,20,0,5" FontWeight="Bold"/>
        <DataGrid ItemsSource="{Binding Items}" AutoGenerateColumns="False" CanUserAddRows="False" Margin="0,0,0,10">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*"/>
                <DataGridTextColumn Header="Quantity" Binding="{Binding Quantity}" Width="*"/>
                <DataGridTextColumn Header="Price" Binding="{Binding Price}" Width="*"/>
                <DataGridTemplateColumn Header="Action">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Button Content="Delete"
                                    Command="{Binding DataContext.RemoveItemCommand, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                    CommandParameter="{Binding}" />
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>

        <Button Content="Add Item" Command="{Binding AddItemCommand}" Width="100" />

        <Button Content="Save" Command="{Binding SaveCommand}" Margin="0,20,0,0" Width="100"/>
    </StackPanel>
</Window>
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

public partial class PurchaseOrderViewModel : ObservableObject
{
    [ObservableProperty]
    private string orderNumber;

    [ObservableProperty]
    private DateTime date = DateTime.Now;

    public ObservableCollection<ItemRow> Items { get; set; } = new();

    [RelayCommand]
    private void AddItem()
    {
        Items.Add(new ItemRow());
    }

    [RelayCommand]
    private void RemoveItem(ItemRow item)
    {
        if (item != null)
            Items.Remove(item);
    }

    [RelayCommand]
    private void Save()
    {
        var order = new PurchaseOrder
        {
            OrderNumber = OrderNumber,
            Date = Date,
            Items = Items.ToList() // convert ObservableCollection to List
        };

        // TODO: Save `order` to database or further processing
    }
}public class PurchaseOrder
{
    public string OrderNumber { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public List<ItemRow> Items { get; set; } = new();
}
public class ItemRow
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
public class GenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _set;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync() => await _set.ToListAsync();

    public async Task AddAsync(T entity)
    {
        _set.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _set.Remove(entity);
        await _context.SaveChangesAsync();
    }
}using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<ItemRow> ItemRows => Set<ItemRow>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
using System.ComponentModel.DataAnnotations;

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;

    public List<ItemRow> Items { get; set; } = new();

    public decimal GrandTotal => Items?.Sum(i => i.Total) ?? 0;
}
public class ItemRow
{
    public int Id { get; set; } // for EF
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal Total => Quantity * Price;

    public int PurchaseOrderId { get; set; }
}