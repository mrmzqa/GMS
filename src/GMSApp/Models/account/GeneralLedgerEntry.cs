using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.account
{
    public class GeneralLedgerEntry
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty; // e.g., Invoice #, Payment #
        public string Description { get; set; } = string.Empty;

        public ICollection<GeneralLedgerLine> Lines { get; set; } = new List<GeneralLedgerLine>();
    }
}
