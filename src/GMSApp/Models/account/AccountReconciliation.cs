using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.account
{
    public class AccountReconciliation
    {
        public int Id { get; set; }
        public int ChartOfAccountId { get; set; } // Usually Bank Accounts
        public ChartOfAccount ChartOfAccount { get; set; }

        public DateTime ReconciliationDate { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal LedgerBalance { get; set; }
        public bool IsReconciled { get; set; }

        public ICollection<ReconciliationItem> Items { get; set; } = new List<ReconciliationItem>();
    }
}
