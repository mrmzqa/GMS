using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.invoice
{
    public enum InvoiceStatus
    {
        Draft = 0,
        Pending = 1,
        Paid = 2,
        PartiallyPaid = 3,
        Cancelled = 4,
        Overdue = 5
    }
}
