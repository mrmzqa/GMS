using GMSApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.purchase
{
    public class Purchaseorder
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

        public ICollection<Purchaseorderline> Lines { get; set; } = new List<Purchaseorderline>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }
}
