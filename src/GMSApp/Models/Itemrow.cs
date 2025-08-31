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


