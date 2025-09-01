/*using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GMSApp.Models
{
    // ---------------- ENUMS ----------------
 
    public enum PurchaseOrderStatus
    {
        Draft = 0,
        PendingApproval = 1,
        Approved = 2,
        PartiallyDelivered = 3,
        Delivered = 4,
        Cancelled = 5,
        Closed = 6
    }

    public enum PaymentMethod
    {
        Cash = 0,
        BankTransfer = 1,
        Cheque = 2,
        Credit = 3
    }

    public enum Currency
    {
        QAR = 0,
        USD = 1,
        EUR = 2
    }

    public enum InvoiceStatus
    {
        Draft = 0,
        Pending = 1,
        Paid = 2,
        PartiallyPaid = 3,
        Cancelled = 4,
        Overdue = 5
    }
    // ---------------- ADDRESS ----------------
    public class Address
    {
        public int Id { get; set; }

        [MaxLength(250)]
        public string Line1 { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Line2 { get; set; }

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(50)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; } = "Qatar";
    }

    // ---------------- VENDOR ----------------
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

        public ICollection<PurchaseOrder>? PurchaseOrders { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
        public ICollection<PaymentReceipt>? PaymentReceipts { get; set; }
    }

    // ---------------- PURCHASE ORDER ----------------
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string PONumber { get; set; } = string.Empty; // e.g., PO-2025-001

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public Currency Currency { get; set; } = Currency.QAR;
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? IBAN { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [MaxLength(500)]
        public string? DeliveryLocation { get; set; }

        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    public class PurchaseOrderLine
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        [Required, MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; } = "pc";

        [MaxLength(250)]
        public string? Notes { get; set; }

        // Delivery tracking
        public decimal QuantityDelivered { get; set; }

        [NotMapped]
        public decimal QuantityPending => Quantity - QuantityDelivered;
    }


 


    // ---------------- PAYMENT RECEIPT ----------------
    public class PaymentReceipt
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty; // e.g., RCPT-2025-001

        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? ReferenceNumber { get; set; } // e.g., Cheque No, Transfer Ref

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? CreatedBy { get; set; }
    }

    // ---------------- EXTENSIONS ----------------
    public static class PurchaseOrderExtensions
    {
        public static void RecalculateTotals(this PurchaseOrder po)
        {
            if (po == null) return;

            decimal subtotal = 0m;
            foreach (var line in po.Lines)
            {
                line.LineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
                subtotal += line.LineTotal;
            }

            po.SubTotal = Math.Round(subtotal, 2);
            po.Total = Math.Round(po.SubTotal - po.Discount + po.Tax, 2);
        }
    }




}*/
