
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Finance
{
    public partial class ChartOfAccountViewModel : ObservableObject
    {
        private readonly IRepository<ChartOfAccount> _repo;

        public ObservableCollection<ChartOfAccount> Accounts { get; } = new();

        public ChartOfAccountViewModel(IRepository<ChartOfAccount> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private ChartOfAccount? selectedAccount;

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Accounts.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var a in list.OrderBy(x => x.AccountCode))
                {
                    Accounts.Add(a);
                }

                SelectedAccount = Accounts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load chart of accounts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var newAcc = new ChartOfAccount
            {
                AccountCode = string.Empty,
                AccountName = string.Empty,
                AccountType = AccountType.Asset,
                IsActive = true
            };

            Accounts.Add(newAcc);
            SelectedAccount = newAcc;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedAccount == null) return;

            try
            {
                var detached = new ChartOfAccount
                {
                    Id = SelectedAccount.Id,
                    AccountCode = SelectedAccount.AccountCode?.Trim() ?? string.Empty,
                    AccountName = SelectedAccount.AccountName?.Trim() ?? string.Empty,
                    AccountType = SelectedAccount.AccountType,
                    IsActive = SelectedAccount.IsActive,
                    ParentAccountId = SelectedAccount.ParentAccountId
                };

                if (detached.Id == 0)
                    await _repo.AddAsync(detached);
                else
                    await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedAccount = Accounts.FirstOrDefault(a => a.AccountCode == detached.AccountCode) ?? SelectedAccount;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedAccount == null) return;
            var ok = MessageBox.Show("Delete selected account?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedAccount.Id == 0)
                {
                    Accounts.Remove(SelectedAccount);
                    SelectedAccount = Accounts.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedAccount.Id);
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModify() => SelectedAccount != null;
    }
}

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
} 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Accounting
{
    // A single viewmodel that exposes collections & commands for ChartOfAccount,
    // GeneralLedgerEntry (with lines), AccountsReceivable, AccountsPayable, AccountReconciliation.
    // Repositories follow the IRepository<T> convention used elsewhere in the app.
    public partial class AccountingViewModel : ObservableObject
    {
        private readonly IRepository<ChartOfAccount> _coaRepo;
        private readonly IRepository<GeneralLedgerEntry> _glRepo;
        private readonly IRepository<AccountsReceivable> _arRepo;
        private readonly IRepository<AccountsPayable> _apRepo;
        private readonly IRepository<AccountReconciliation> _reconRepo;

        public AccountingViewModel(
            IRepository<ChartOfAccount> coaRepo,
            IRepository<GeneralLedgerEntry> glRepo,
            IRepository<AccountsReceivable> arRepo,
            IRepository<AccountsPayable> apRepo,
            IRepository<AccountReconciliation> reconRepo)
        {
            _coaRepo = coaRepo ?? throw new ArgumentNullException(nameof(coaRepo));
            _glRepo = glRepo ?? throw new ArgumentNullException(nameof(glRepo));
            _arRepo = arRepo ?? throw new ArgumentNullException(nameof(arRepo));
            _apRepo = apRepo ?? throw new ArgumentNullException(nameof(apRepo));
            _reconRepo = reconRepo ?? throw new ArgumentNullException(nameof(reconRepo));

            _ = LoadAllAsync();
        }

        #region Chart Of Accounts

        public ObservableCollection<ChartOfAccount> ChartOfAccounts { get; } = new();

        [ObservableProperty]
        private ChartOfAccount? selectedChartOfAccount;

        [RelayCommand]
        public async Task LoadCoaAsync()
        {
            try
            {
                ChartOfAccounts.Clear();
                var list = await _coaRepo.GetAllAsync();
                foreach (var a in list) ChartOfAccounts.Add(a);
                SelectedChartOfAccount = ChartOfAccounts.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load COA failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddCoaAsync()
        {
            var a = new ChartOfAccount { AccountCode = string.Empty, AccountName = string.Empty, AccountType = AccountType.Asset, IsActive = true };
            ChartOfAccounts.Add(a);
            SelectedChartOfAccount = a;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyCoa))]
        public async Task SaveCoaAsync()
        {
            if (SelectedChartOfAccount == null) return;
            try
            {
                var detached = new ChartOfAccount
                {
                    Id = SelectedChartOfAccount.Id,
                    AccountCode = SelectedChartOfAccount.AccountCode?.Trim() ?? string.Empty,
                    AccountName = SelectedChartOfAccount.AccountName?.Trim() ?? string.Empty,
                    AccountType = SelectedChartOfAccount.AccountType,
                    IsActive = SelectedChartOfAccount.IsActive,
                    ParentAccountId = SelectedChartOfAccount.ParentAccountId
                };

                if (detached.Id == 0) await _coaRepo.AddAsync(detached); else await _coaRepo.UpdateAsync(detached);
                await LoadCoaAsync();
                SelectedChartOfAccount = ChartOfAccounts.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedChartOfAccount;
            }
            catch (Exception ex) { MessageBox.Show($"Save COA failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyCoa))]
        public async Task DeleteCoaAsync()
        {
            if (SelectedChartOfAccount == null) return;
            if (SelectedChartOfAccount.Id == 0)
            {
                ChartOfAccounts.Remove(SelectedChartOfAccount);
                SelectedChartOfAccount = ChartOfAccounts.FirstOrDefault();
                return;
            }

            var ok = MessageBox.Show("Delete account?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try
            {
                await _coaRepo.DeleteAsync(SelectedChartOfAccount.Id);
                await LoadCoaAsync();
            }
            catch (Exception ex) { MessageBox.Show($"Delete COA failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanModifyCoa() => SelectedChartOfAccount != null;

        #endregion

        #region General Ledger (Entries + Lines)

        public ObservableCollection<GeneralLedgerEntry> GeneralLedgerEntries { get; } = new();

        [ObservableProperty]
        private GeneralLedgerEntry? selectedGlEntry;

        // editable lines for selected entry (two-way binding)
        public ObservableCollection<GeneralLedgerLineEditable> GlLines { get; } = new();

        partial void OnSelectedGlEntryChanged(GeneralLedgerEntry? value)
        {
            GlLines.Clear();
            if (value?.Lines != null)
            {
                foreach (var l in value.Lines)
                {
                    var ed = new GeneralLedgerLineEditable(l);
                    ed.PropertyChanged += GlLine_PropertyChanged;
                    GlLines.Add(ed);
                }
            }
            NotifyGlCommands();
        }

        private void GlLine_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeneralLedgerLineEditable.Debit) || e.PropertyName == nameof(GeneralLedgerLineEditable.Credit))
            {
                // nothing else required here, but UI can display totals bound to properties
                OnPropertyChanged(nameof(GlTotalDebit));
                OnPropertyChanged(nameof(GlTotalCredit));
            }
        }

        public decimal GlTotalDebit => GlLines.Sum(x => x.Debit);
        public decimal GlTotalCredit => GlLines.Sum(x => x.Credit);

        [RelayCommand]
        public async Task LoadGlAsync()
        {
            try
            {
                GeneralLedgerEntries.Clear();
                var list = await _glRepo.GetAllAsync();
                foreach (var e in list) GeneralLedgerEntries.Add(e);
                SelectedGlEntry = GeneralLedgerEntries.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load GL failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddGlEntryAsync()
        {
            var e = new GeneralLedgerEntry { EntryDate = DateTime.UtcNow, ReferenceNumber = string.Empty, Description = string.Empty, Lines = new System.Collections.Generic.List<GeneralLedgerLine>() };
            GeneralLedgerEntries.Add(e);
            SelectedGlEntry = e;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public async Task SaveGlEntryAsync()
        {
            if (SelectedGlEntry == null) return;
            try
            {
                // Map editable lines back to model
                SelectedGlEntry.Lines = GlLines.Select(l => new GeneralLedgerLine
                {
                    Id = l.Id,
                    ChartOfAccountId = l.ChartOfAccountId,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList();

                // Basic validation: debits must equal credits
                if (Math.Round(SelectedGlEntry.Lines.Sum(x => x.Debit), 2) != Math.Round(SelectedGlEntry.Lines.Sum(x => x.Credit), 2))
                {
                    var res = MessageBox.Show("Debits and Credits are not equal. Save anyway?", "Validation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res != MessageBoxResult.Yes) return;
                }

                var detached = new GeneralLedgerEntry
                {
                    Id = SelectedGlEntry.Id,
                    EntryDate = SelectedGlEntry.EntryDate,
                    ReferenceNumber = SelectedGlEntry.ReferenceNumber,
                    Description = SelectedGlEntry.Description,
                    Lines = SelectedGlEntry.Lines
                };

                if (detached.Id == 0) await _glRepo.AddAsync(detached); else await _glRepo.UpdateAsync(detached);
                await LoadGlAsync();
                SelectedGlEntry = GeneralLedgerEntries.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedGlEntry;
            }
            catch (Exception ex) { MessageBox.Show($"Save GL entry failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public async Task DeleteGlEntryAsync()
        {
            if (SelectedGlEntry == null) return;
            if (SelectedGlEntry.Id == 0)
            {
                GeneralLedgerEntries.Remove(SelectedGlEntry);
                SelectedGlEntry = GeneralLedgerEntries.FirstOrDefault();
                return;
            }
            var ok = MessageBox.Show("Delete GL entry?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _glRepo.DeleteAsync(SelectedGlEntry.Id); await LoadGlAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete GL failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public Task AddGlLineAsync()
        {
            var l = new GeneralLedgerLineEditable { ChartOfAccountId = 0, Debit = 0m, Credit = 0m };
            GlLines.Add(l);
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyGl))]
        public Task RemoveGlLineAsync(GeneralLedgerLineEditable? line)
        {
            if (line == null) return Task.CompletedTask;
            GlLines.Remove(line);
            return Task.CompletedTask;
        }

        private bool CanModifyGl() => SelectedGlEntry != null;

        private void NotifyGlCommands()
        {
            AddGlLineCommand.NotifyCanExecuteChanged();
            RemoveGlLineCommand.NotifyCanExecuteChanged();
            SaveGlEntryCommand.NotifyCanExecuteChanged();
            DeleteGlEntryCommand.NotifyCanExecuteChanged();
        }

        #endregion

        #region Accounts Receivable

        public ObservableCollection<AccountsReceivable> AccountsReceivableList { get; } = new();

        [ObservableProperty]
        private AccountsReceivable? selectedAr;

        [RelayCommand]
        public async Task LoadArAsync()
        {
            try
            {
                AccountsReceivableList.Clear();
                var list = await _arRepo.GetAllAsync();
                foreach (var a in list) AccountsReceivableList.Add(a);
                SelectedAr = AccountsReceivableList.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load AR failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddArAsync()
        {
            var a = new AccountsReceivable { InvoiceDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), InvoiceNumber = string.Empty, Amount = 0m, ReceivedAmount = 0m };
            AccountsReceivableList.Add(a);
            SelectedAr = a;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyAr))]
        public async Task SaveArAsync()
        {
            if (SelectedAr == null) return;
            try
            {
                var detached = new AccountsReceivable
                {
                    Id = SelectedAr.Id,
                    CustomerId = SelectedAr.CustomerId,
                    InvoiceDate = SelectedAr.InvoiceDate,
                    InvoiceNumber = SelectedAr.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedAr.Amount,
                    ReceivedAmount = SelectedAr.ReceivedAmount,
                    DueDate = SelectedAr.DueDate,
                    Status = SelectedAr.Status
                };

                // set status
                detached.Status = detached.ReceivedAmount == 0 ? InvoiceStatus.Unpaid :
                                  detached.ReceivedAmount < detached.Amount ? InvoiceStatus.PartiallyPaid :
                                  InvoiceStatus.Paid;

                if (detached.Id == 0) await _arRepo.AddAsync(detached); else await _arRepo.UpdateAsync(detached);
                await LoadArAsync();
                SelectedAr = AccountsReceivableList.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedAr;
            }
            catch (Exception ex) { MessageBox.Show($"Save AR failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyAr))]
        public async Task DeleteArAsync()
        {
            if (SelectedAr == null) return;
            if (SelectedAr.Id == 0) { AccountsReceivableList.Remove(SelectedAr); SelectedAr = AccountsReceivableList.FirstOrDefault(); return; }
            var ok = MessageBox.Show("Delete AR record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _arRepo.DeleteAsync(SelectedAr.Id); await LoadArAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete AR failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanModifyAr() => SelectedAr != null;

        #endregion

        #region Accounts Payable

        public ObservableCollection<AccountsPayable> AccountsPayableList { get; } = new();

        [ObservableProperty]
        private AccountsPayable? selectedAp;

        [RelayCommand]
        public async Task LoadApAsync()
        {
            try
            {
                AccountsPayableList.Clear();
                var list = await _apRepo.GetAllAsync();
                foreach (var a in list) AccountsPayableList.Add(a);
                SelectedAp = AccountsPayableList.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load AP failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddApAsync()
        {
            var a = new AccountsPayable { InvoiceDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), InvoiceNumber = string.Empty, Amount = 0m, PaidAmount = 0m };
            AccountsPayableList.Add(a);
            SelectedAp = a;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyAp))]
        public async Task SaveApAsync()
        {
            if (SelectedAp == null) return;
            try
            {
                var detached = new AccountsPayable
                {
                    Id = SelectedAp.Id,
                    VendorId = SelectedAp.VendorId,
                    InvoiceDate = SelectedAp.InvoiceDate,
                    InvoiceNumber = SelectedAp.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedAp.Amount,
                    PaidAmount = SelectedAp.PaidAmount,
                    DueDate = SelectedAp.DueDate,
                    Status = SelectedAp.Status
                };

                detached.Status = detached.PaidAmount == 0 ? InvoiceStatus.Unpaid :
                                   detached.PaidAmount < detached.Amount ? InvoiceStatus.PartiallyPaid :
                                   InvoiceStatus.Paid;

                if (detached.Id == 0) await _apRepo.AddAsync(detached); else await _apRepo.UpdateAsync(detached);
                await LoadApAsync();
                SelectedAp = AccountsPayableList.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedAp;
            }
            catch (Exception ex) { MessageBox.Show($"Save AP failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyAp))]
        public async Task DeleteApAsync()
        {
            if (SelectedAp == null) return;
            if (SelectedAp.Id == 0) { AccountsPayableList.Remove(SelectedAp); SelectedAp = AccountsPayableList.FirstOrDefault(); return; }
            var ok = MessageBox.Show("Delete AP record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _apRepo.DeleteAsync(SelectedAp.Id); await LoadApAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete AP failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private bool CanModifyAp() => SelectedAp != null;

        #endregion

        #region Account Reconciliation

        public ObservableCollection<AccountReconciliation> Reconciliations { get; } = new();

        [ObservableProperty]
        private AccountReconciliation? selectedReconciliation;

        // Items presented for matching (editable boolean)
        public ObservableCollection<ReconciliationItemEditable> ReconItems { get; } = new();

        partial void OnSelectedReconciliationChanged(AccountReconciliation? value)
        {
            ReconItems.Clear();
            if (value?.Items != null)
            {
                foreach (var i in value.Items)
                {
                    var ed = new ReconciliationItemEditable(i);
                    ed.PropertyChanged += ReconItem_PropertyChanged;
                    ReconItems.Add(ed);
                }
            }
            NotifyReconCommands();
        }

        private void ReconItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReconciliationItemEditable.IsMatched))
            {
                // update reconciled indicator
                var matchedCount = ReconItems.Count(x => x.IsMatched);
                SelectedReconciliation!.IsReconciled = matchedCount > 0 && Math.Abs(SelectedReconciliation!.LedgerBalance - ReconItems.Where(x => x.IsMatched).Sum(x => x.Amount)) < 0.01m;
                OnPropertyChanged(nameof(SelectedReconciliation));
            }
        }

        [RelayCommand]
        public async Task LoadReconAsync()
        {
            try
            {
                Reconciliations.Clear();
                var list = await _reconRepo.GetAllAsync();
                foreach (var r in list) Reconciliations.Add(r);
                SelectedReconciliation = Reconciliations.FirstOrDefault();
            }
            catch (Exception ex) { MessageBox.Show($"Load Reconciliations failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand]
        public Task AddReconAsync()
        {
            var r = new AccountReconciliation { ReconciliationDate = DateTime.UtcNow, ChartOfAccountId = 0, StatementBalance = 0m, LedgerBalance = 0m, IsReconciled = false };
            Reconciliations.Add(r);
            SelectedReconciliation = r;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifyRecon))]
        public async Task SaveReconAsync()
        {
            if (SelectedReconciliation == null) return;
            try
            {
                // Map edited items back to model
                SelectedReconciliation.Items = ReconItems.Select(i => new ReconciliationItem
                {
                    Id = i.Id,
                    GeneralLedgerLineId = i.GeneralLedgerLineId,
                    IsMatched = i.IsMatched
                }).ToList();

                var detached = new AccountReconciliation
                {
                    Id = SelectedReconciliation.Id,
                    ChartOfAccountId = SelectedReconciliation.ChartOfAccountId,
                    ReconciliationDate = SelectedReconciliation.ReconciliationDate,
                    StatementBalance = SelectedReconciliation.StatementBalance,
                    LedgerBalance = SelectedReconciliation.LedgerBalance,
                    Items = SelectedReconciliation.Items
                };

                if (detached.Id == 0) await _reconRepo.AddAsync(detached); else await _reconRepo.UpdateAsync(detached);
                await LoadReconAsync();
                SelectedReconciliation = Reconciliations.FirstOrDefault(x => x.Id == detached.Id) ?? SelectedReconciliation;
            }
            catch (Exception ex) { MessageBox.Show($"Save reconciliation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        [RelayCommand(CanExecute = nameof(CanModifyRecon))]
        public async Task DeleteReconAsync()
        {
            if (SelectedReconciliation == null) return;
            if (SelectedReconciliation.Id == 0) { Reconciliations.Remove(SelectedReconciliation); SelectedReconciliation = Reconciliations.FirstOrDefault(); return; }
            var ok = MessageBox.Show("Delete reconciliation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;
            try { await _reconRepo.DeleteAsync(SelectedReconciliation.Id); await LoadReconAsync(); }
            catch (Exception ex) { MessageBox.Show($"Delete reconciliation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // Quick helper that loads unreconciled GL lines for selected chart account into ReconItems (this requires repository support in real app)
        [RelayCommand(CanExecute = nameof(CanModifyRecon))]
        public Task LoadUnreconciledLinesAsync()
        {
            if (SelectedReconciliation == null) return Task.CompletedTask;

            // NOTE: This sample assumes _glRepo.GetAllAsync returns entries with lines and ChartOfAccountId set.
            // In a real app filter on ChartOfAccountId and whether line already in previous reconciliations.
            ReconItems.Clear();
            var lines = GeneralLedgerEntries.SelectMany(e => e.Lines ?? new System.Collections.Generic.List<GeneralLedgerLine>())
                                           .Where(l => l.ChartOfAccountId == SelectedReconciliation.ChartOfAccountId)
                                           .Select(l => new ReconciliationItemEditable
                                           {
                                               Id = 0,
                                               GeneralLedgerLineId = l.Id,
                                               Description = $"{l.Id} | D:{l.Debit:N2} C:{l.Credit:N2}",
                                               Amount = l.Debit - l.Credit,
                                               IsMatched = false
                                           });

            foreach (var r in lines) ReconItems.Add(r);

            // compute ledger balance from these lines
            SelectedReconciliation.LedgerBalance = ReconItems.Sum(x => x.Amount);
            OnPropertyChanged(nameof(SelectedReconciliation));
            return Task.CompletedTask;
        }

        private bool CanModifyRecon() => SelectedReconciliation != null;

        private void NotifyReconCommands()
        {
            LoadUnreconciledLinesCommand.NotifyCanExecuteChanged();
            SaveReconCommand.NotifyCanExecuteChanged();
            DeleteReconCommand.NotifyCanExecuteChanged();
        }

        #endregion

        #region Helpers + LoadAll

        [RelayCommand]
        public async Task LoadAllAsync()
        {
            await Task.WhenAll(LoadCoaAsync(), LoadGlAsync(), LoadArAsync(), LoadApAsync(), LoadReconAsync());
        }

        #endregion

        #region Editable helper classes

        public class GeneralLedgerLineEditable : ObservableObject
        {
            public GeneralLedgerLineEditable() { }
            public GeneralLedgerLineEditable(GeneralLedgerLine src)
            {
                Id = src.Id;
                ChartOfAccountId = src.ChartOfAccountId;
                Debit = src.Debit;
                Credit = src.Credit;
            }

            [ObservableProperty] private int id;
            [ObservableProperty] private int chartOfAccountId;
            [ObservableProperty] private decimal debit;
            partial void OnDebitChanged(decimal value) => OnPropertyChanged(nameof(Balance));
            [ObservableProperty] private decimal credit;
            partial void OnCreditChanged(decimal value) => OnPropertyChanged(nameof(Balance));
            public decimal Balance => Debit - Credit;
        }

        public class ReconciliationItemEditable : ObservableObject
        {
            public ReconciliationItemEditable() { }
            public ReconciliationItemEditable(ReconciliationItem src)
            {
                Id = src.Id;
                GeneralLedgerLineId = src.GeneralLedgerLineId;
                IsMatched = src.IsMatched;
            }

            [ObservableProperty] private int id;
            [ObservableProperty] private int generalLedgerLineId;
            [ObservableProperty] private string description = string.Empty;
            [ObservableProperty] private decimal amount;
            [ObservableProperty] private bool isMatched;
        }

        #endregion
    }
}