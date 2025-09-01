using GMSApp.Models.Enums;
using GMSApp.Models.invoice;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models
{
    public class PaymentReceipt
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty; // e.g., RCPT-2025-001

        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        public int? VendorId { get; set; }
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
}