using System.ComponentModel.DataAnnotations;

namespace GMSApp.Models;

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