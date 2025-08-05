using System.ComponentModel.DataAnnotations;

namespace GMSApp.Models;
public class ItemRow
{
    public int Id { get; set; } // for EF
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public decimal Total => Quantity * Price;

    public int PurchaseOrderId { get; set; }
}


