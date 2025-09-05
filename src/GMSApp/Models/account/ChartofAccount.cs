using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.account
{
    public class ChartOfAccount
    {
        [Key]
        public int Id { get; set; }
        public string AccountCode { get; set; } = string.Empty; // e.g., 1010
        public string AccountName { get; set; } = string.Empty; // e.g., Cash
        public AccountType AccountType { get; set; } // Asset, Liability, Equity, Revenue, Expense
        public bool IsActive { get; set; } = true;

        // Hierarchy (Parent-Child)
        public int? ParentAccountId { get; set; }
        public ChartOfAccount? ParentAccount { get; set; }
        public ICollection<ChartOfAccount> SubAccounts { get; set; } = new List<ChartOfAccount>();
    }
}
