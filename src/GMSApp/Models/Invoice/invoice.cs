using GMSApp.Models.Enums;
using GMSApp.Models.purchase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.invoice
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty; // e.g., INV-2025-001

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public int? VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [NotMapped]
        public decimal Balance => Total - AmountPaid;

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public ICollection<Invoiceline> Lines { get; set; } = new List<Invoiceline>();
        public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }
}
