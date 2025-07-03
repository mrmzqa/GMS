using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS.Models.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int JobOrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public JobOrder JobOrder { get; set; }
    }
}
