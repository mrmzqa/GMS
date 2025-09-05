using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.inventory
{
    public enum StockTransactionType
    {
        Purchase = 1,
        JobUsage = 2,
        Adjustment = 3,
        Return = 4
    }
}
