using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApp.Models
{
    public enum TransactionType { Receivable, Payable }
    public enum TransactionStatus { Pending, Paid }

    public class AccountTransaction
    {
        [Key]
        public Guid TransactionId { get; set; } = Guid.NewGuid();

        [Required]
        public TransactionType Type { get; set; }

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    }
}