using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.invoice
{
    public class Invoiceline
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
}
