using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.purchase
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
}
