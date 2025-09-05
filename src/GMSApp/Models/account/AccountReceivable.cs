using GMSApp.Models.invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.account
{
    public class AccountsReceivable
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } // from Vendors table
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal ReceivedAmount { get; set; } = 0;
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    }
}
