using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System.Collections.ObjectModel;
using System.IO;
namespace GMSApp.ViewModels;
public partial class PurchaseOrderViewModel : ObservableObject
{
    private readonly IRepository<PurchaseOrder> _repository;
    private readonly IGenericPdfGenerator<PurchaseOrder> _GenericPdfGenerator;
    public PurchaseOrderViewModel(IRepository<PurchaseOrder> repo,IGenericPdfGenerator<PurchaseOrder>genericPdfGenerator)
    {
        _repository = repo;
        _GenericPdfGenerator = genericPdfGenerator;
    }

    [ObservableProperty]
    private string orderNumber;

    [ObservableProperty]
    private DateTime date = DateTime.Now;

   

  

    public ObservableCollection<ItemRow> Items { get; set; } = new();

    public decimal Total => Items.Sum(x => x.Total);

    [RelayCommand]
    private void AddItem() => Items.Add(new ItemRow());

    [RelayCommand]
    private void RemoveItem(ItemRow item)
    {
        if (item != null) Items.Remove(item);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var po = new PurchaseOrder
        {
           OrderNumber = orderNumber,
           Date = date,
            Items = Items.ToList()

        };

        await _repository.AddAsync(po);
        
    }
 

}