Perfect 👍 thanks for confirming. I’ll merge everything into one full model file so your Garage Management System has:

Vendor, Address

PurchaseOrder, PurchaseOrderLine

Invoice, InvoiceLine

PaymentReceipt

Extension helpers for PO & Invoice

GarageDbContext with all entity sets + relationships


Here’s the unified code:

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GarageManagement.Models
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
        public Vendor? Vendor { get; set; }

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

    // ---------------- INVOICE ----------------
    public class Invoice
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty; // e.g., INV-2025-001

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
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

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    public class InvoiceLine
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

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

    public static class InvoiceExtensions
    {
        public static void RecalculateTotals(this Invoice invoice)
        {
            if (invoice == null) return;

            decimal subtotal = 0m;
            foreach (var line in invoice.Lines)
            {
                line.LineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
                subtotal += line.LineTotal;
            }

            invoice.SubTotal = Math.Round(subtotal, 2);
            invoice.Total = Math.Round(invoice.SubTotal - invoice.Discount + invoice.Tax, 2);

            if (invoice.AmountPaid == 0)
                invoice.Status = InvoiceStatus.Pending;
            else if (invoice.AmountPaid < invoice.Total)
                invoice.Status = InvoiceStatus.PartiallyPaid;
            else if (invoice.AmountPaid >= invoice.Total)
                invoice.Status = InvoiceStatus.Paid;
        }
    }

    // ---------------- DB CONTEXT ----------------
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options) { }

        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PurchaseOrder ↔ Lines
            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(p => p.Lines)
                .WithOne(l => l.PurchaseOrder)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vendor ↔ PurchaseOrders
            modelBuilder.Entity<Vendor>()
                .HasMany(v => v.PurchaseOrders)
                .WithOne(p => p.Vendor)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice ↔ Lines
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.Lines)
                .WithOne(l => l.Invoice)
                .HasForeignKey(l => l.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vendor ↔ Invoices
            modelBuilder.Entity<Vendor>()
                .HasMany(v => v.Invoices)
                .WithOne(i => i.Vendor)
                .HasForeignKey(i => i.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Vendor ↔ PaymentReceipts
            modelBuilder.Entity<Vendor>()
                .HasMany(v => v.PaymentReceipts)
                .WithOne(r => r.Vendor)
                .HasForeignKey(r => r.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice ↔ PaymentReceipts
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.PaymentReceipts)
                .WithOne(r => r.Invoice)
                .HasForeignKey(r => r.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


---

⚡ This version is plug-and-play:

Drop it in your Models project.

Run Add-Migration Initial + Update-Database in EF Core, and you’ll have tables for Vendors, POs, Invoices, and Payments.


Do you also want me to add AccountsPayable/AccountsReceivable aggregates (so you can quickly see per-vendor balance without recalculating every time), or will you compute balances dynamically from Invoices + Payments?

