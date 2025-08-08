using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using GMSApp.Services;
using System.Collections.ObjectModel;
using System.IO;
namespace GMSApp.ViewModels;
public partial class PurchaseOrderViewModel : ObservableObject
{
    private readonly IRepository<PurchaseOrder> _repository;

    public PurchaseOrderViewModel(IRepository<PurchaseOrder> repo)
    {
        _repository = repo;
    }

    [ObservableProperty]
    private string orderNumber;

    [ObservableProperty]
    private DateTime date = DateTime.Now;

    [ObservableProperty]
    private string selectedLanguage = "en";

    partial void OnSelectedLanguageChanged(string value)
    {
        LocalizationService.SetCulture(value);
    }

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

    [RelayCommand]
    private void ExportPdf()
    {
        var po = new PurchaseOrder
        {
            OrderNumber = OrderNumber,
            Date = Date,
            Items = Items.ToList()
        };

        var pdf = PdfService.GeneratePurchaseOrderPdf(po);
        File.WriteAllBytes($"PO_{OrderNumber}.pdf", pdf);
    }
}