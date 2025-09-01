using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.Enums
{
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
}
