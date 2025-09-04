using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.quotation
{
    public enum QuotationStatus
    {
        Draft,
        Sent,
        Approved,
        Rejected,
        ConvertedToJobOrder
    }
}
