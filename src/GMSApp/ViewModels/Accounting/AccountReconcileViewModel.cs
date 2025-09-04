
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Commands;
using GMSApp.Models;
using GMSApp.Models.account;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Accounting
{
    public partial class AccountReconcileViewModel : ObservableObject
    {
        private readonly IRepository<AccountReconciliation> _repo;

        public ObservableCollection<AccountReconciliation> Reconciliations { get; } = new();

        public AccountReconcileViewModel(IRepository<AccountReconciliation> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private AccountReconciliation? selectedReconciliation;

        partial void OnSelectedReconciliationChanged(AccountReconciliation? value)
        {
            Items.Clear();
            if (value != null)
            {
                foreach (var it in value.Items)
                {
                    var ed = new ReconciliationItemEditable
                    {
                        Id = it.Id,
                        GeneralLedgerLineId = it.GeneralLedgerLineId,
                        IsMatched = it.IsMatched,
                        Amount = 0m // amount can be set by user; repository could enrich this if desired
                    };
                    ed.PropertyChanged += Item_PropertyChanged;
                    Items.Add(ed);
                }
            }

            OnPropertyChanged(nameof(LedgerBalance));
            OnPropertyChanged(nameof(IsReconciled));
            NotifyCommands();
        }

        public ObservableCollection<ReconciliationItemEditable> Items { get; } = new();

        [ObservableProperty]
        private ReconciliationItemEditable? selectedItem;

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReconciliationItemEditable.IsMatched) ||
                e.PropertyName == nameof(ReconciliationItemEditable.Amount))
            {
                OnPropertyChanged(nameof(LedgerBalance));
                OnPropertyChanged(nameof(IsReconciled));
            }
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AddItemCommand.NotifyCanExecuteChanged();
            RemoveItemCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Reconciliations.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var r in list) Reconciliations.Add(r);
                SelectedReconciliation = Reconciliations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load reconciliations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var rec = new AccountReconciliation
            {
                ReconciliationDate = DateTime.UtcNow,
                StatementBalance = 0m,
                LedgerBalance = 0m,
                IsReconciled = false
            };
            Reconciliations.Add(rec);
            SelectedReconciliation = rec;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedReconciliation == null) return;

            try
            {
                // create detached copy
                var detached = new AccountReconciliation
                {
                    Id = SelectedReconciliation.Id,
                    ChartOfAccountId = SelectedReconciliation.ChartOfAccountId,
                    ReconciliationDate = SelectedReconciliation.ReconciliationDate,
                    StatementBalance = SelectedReconciliation.StatementBalance,
                    LedgerBalance = LedgerBalance,
                    IsReconciled = IsReconciled,
                    Items = Items.Select(i => new ReconciliationItem
                    {
                        Id = i.Id,
                        GeneralLedgerLineId = i.GeneralLedgerLineId,
                        IsMatched = i.IsMatched
                    }).ToList()
                };

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedReconciliation = Reconciliations.FirstOrDefault(r => r.Id == detached.Id) ?? SelectedReconciliation;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save reconciliation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedReconciliation == null) return;

            var confirm = MessageBox.Show("Delete selected reconciliation?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedReconciliation.Id == 0)
                {
                    Reconciliations.Remove(SelectedReconciliation);
                    SelectedReconciliation = Reconciliations.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedReconciliation.Id);
                    await LoadAsync();
                    SelectedReconciliation = Reconciliations.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete reconciliation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddItem()
        {
            var it = new ReconciliationItemEditable
            {
                GeneralLedgerLineId = 0,
                Amount = 0m,
                IsMatched = false
            };
            it.PropertyChanged += Item_PropertyChanged;
            Items.Add(it);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveItem(ReconciliationItemEditable? it)
        {
            if (it == null) return;
            it.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(it);
            OnPropertyChanged(nameof(LedgerBalance));
            OnPropertyChanged(nameof(IsReconciled));
        }

        public decimal LedgerBalance => Math.Round(Items.Where(i => i.IsMatched).Sum(i => i.Amount), 2);

        public bool IsReconciled
        {
            get
            {
                if (SelectedReconciliation == null) return false;
                return Math.Abs(LedgerBalance - SelectedReconciliation.StatementBalance) < 0.01m;
            }
        }

        private bool CanModify() => SelectedReconciliation != null;

        public partial class ReconciliationItemEditable : ObservableObject
        {
            [ObservableProperty] private int id;
            [ObservableProperty] private int generalLedgerLineId;
            [ObservableProperty] private decimal amount;
            [ObservableProperty] private bool isMatched;
        }
    }
}