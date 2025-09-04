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

public class ReconciliationItem
{
    public int Id { get; set; }
    public int AccountReconciliationId { get; set; }
    public AccountReconciliation AccountReconciliation { get; set; }

    public int GeneralLedgerLineId { get; set; }
    public GeneralLedgerLine GeneralLedgerLine { get; set; }

    public bool IsMatched { get; set; }
}public class AccountsReceivable
{
    public int Id { get; set; }
    public int CustomerId { get; set; } // from Vendors table
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ReceivedAmount { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
}public class AccountsPayable
{
    public int Id { get; set; }
    public int VendorId { get; set; } // from Vendors table
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
}

public enum InvoiceStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overdue
}public class GeneralLedgerEntry
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // e.g., Invoice #, Payment #
    public string Description { get; set; } = string.Empty;

    public ICollection<GeneralLedgerLine> Lines { get; set; } = new List<GeneralLedgerLine>();
}

public class GeneralLedgerLine
{
    public int Id { get; set; }
    public int GeneralLedgerEntryId { get; set; }
    public GeneralLedgerEntry GeneralLedgerEntry { get; set; }

    public int ChartOfAccountId { get; set; }
    public ChartOfAccount ChartOfAccount { get; set; }

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}public class ChartOfAccount
{
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

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
} Create a viewmodel and view for this all