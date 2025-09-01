using GMSApp.Models.purchase;
using GMSApp.Views.Job;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GMSApp.Models;
public class Vendor
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? CRNumber { get; set; } // Commercial Registration number

    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public ICollection<Purchaseorder>? Purchaseorders { get; set; }
    public ICollection<Models.invoice.Invoice>? Invoices { get; set; }
    public ICollection<PaymentReceipt>? PaymentReceipts { get; set; }
}
