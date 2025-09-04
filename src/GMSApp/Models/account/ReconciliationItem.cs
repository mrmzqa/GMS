using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.account
{
    public class ReconciliationItem
    { 
        public int Id { get; set; }
        public int AccountReconciliationId { get; set; }
        public AccountReconciliation? AccountReconciliation { get; set; }

        public int GeneralLedgerLineId { get; set; }
        public GeneralLedgerLine? GeneralLedgerLine { get; set; }

        public bool IsMatched { get; set; }
    }
}