using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models
{
    public class PaymentReceipt
    {
        [Key]
        public Guid ReceiptId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public ServiceJob? ServiceJob { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal AmountPaid { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public DateTime DatePaid { get; set; } = DateTime.UtcNow;

        public string? Remarks { get; set; }
    }
}