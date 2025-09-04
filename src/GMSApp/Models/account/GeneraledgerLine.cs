using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.account
{

    public class GeneralLedgerLine
    {
        public int Id { get; set; }
        public int GeneralLedgerEntryId { get; set; }
        public GeneralLedgerEntry GeneralLedgerEntry { get; set; }

        public int ChartOfAccountId { get; set; }
        public ChartOfAccount ChartOfAccount { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}
