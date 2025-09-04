using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.quotation
{
    public class QuotationItem
    {
        public int Id { get; set; }
        public int RepairQuotationId { get; set; } // FK

        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Computed
        public decimal Total => Quantity * UnitPrice;
    }
}
