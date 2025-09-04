using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Commands;
using GMSApp.Models;
using GMSApp.Models.account;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Accounting
{
    public partial class GeneralLedgerViewModel : ObservableObject
    {
        private readonly IRepository<GeneralLedgerEntry> _repo;
        private readonly IRepository<ChartOfAccount> _coaRepo;

        public ObservableCollection<GeneralLedgerEntry> Entries { get; } = new();
        public ObservableCollection<ChartOfAccount> ChartOfAccounts { get; } = new();

        public GeneralLedgerViewModel(IRepository<GeneralLedgerEntry> repo, IRepository<ChartOfAccount> coaRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _coaRepo = coaRepo ?? throw new ArgumentNullException(nameof(coaRepo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private GeneralLedgerEntry? selectedEntry;

        partial void OnSelectedEntryChanged(GeneralLedgerEntry? value)
        {
            Lines.Clear();
            /*if (value != null)
            {
                foreach (var l in value.Lines)
                {
                    var ed = new EditableLine(l);
                    ed.PropertyChanged += Line_PropertyChanged;
                    Lines.Add(ed);
                }
            }*/

            OnPropertyChanged(nameof(TotalDebits));
            OnPropertyChanged(nameof(TotalCredits));
            NotifyCommands();
        }

        public ObservableCollection<EditableLine> Lines { get; } = new();

        [ObservableProperty]
        private EditableLine? selectedLine;

        private void Line_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditableLine.Debit) || e.PropertyName == nameof(EditableLine.Credit))
            {
                OnPropertyChanged(nameof(TotalDebits));
                OnPropertyChanged(nameof(TotalCredits));
                OnPropertyChanged(nameof(IsBalanced));
            }
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            AddLineCommand.NotifyCanExecuteChanged();
            RemoveLineCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Entries.Clear();
                ChartOfAccounts.Clear();

                var list = await _repo.GetAllAsync();
                foreach (var e in list)
                    Entries.Add(e);

                var coaList = await _coaRepo.GetAllAsync();
                foreach (var c in coaList)
                    ChartOfAccounts.Add(c);

                SelectedEntry = Entries.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load general ledger entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var ent = new GeneralLedgerEntry
            {
                EntryDate = DateTime.UtcNow,
                ReferenceNumber = string.Empty,
                Description = string.Empty,
                Lines = new System.Collections.Generic.List<GeneralLedgerLine>()
            };
            Entries.Add(ent);
            SelectedEntry = ent;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedEntry == null) return;

            try
            {
                // build detached copy
                var detached = new GeneralLedgerEntry
                {
                    Id = SelectedEntry.Id,
                    EntryDate = SelectedEntry.EntryDate,
                    ReferenceNumber = SelectedEntry.ReferenceNumber?.Trim() ?? string.Empty,
                    Description = SelectedEntry.Description?.Trim() ?? string.Empty,
                    Lines = Lines.Select(l => new GeneralLedgerLine
                    {
                        Id = l.Id,
                        ChartOfAccountId = l.ChartOfAccountId,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList()
                };

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedEntry = Entries.FirstOrDefault(e => e.ReferenceNumber == detached.ReferenceNumber) ?? SelectedEntry;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save ledger entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedEntry == null) return;

            var confirm = MessageBox.Show("Delete selected ledger entry?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedEntry.Id == 0)
                {
                    Entries.Remove(SelectedEntry);
                    SelectedEntry = Entries.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedEntry.Id);
                    await LoadAsync();
                    SelectedEntry = Entries.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete ledger entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void AddLine()
        {
            var l = new EditableLine
            {
                ChartOfAccountId = ChartOfAccounts.FirstOrDefault()?.Id ?? 0,
                Debit = 0m,
                Credit = 0m
            };
            l.PropertyChanged += Line_PropertyChanged;
            Lines.Add(l);
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        private void RemoveLine(EditableLine? line)
        {
            if (line == null) return;
            line.PropertyChanged -= Line_PropertyChanged;
            Lines.Remove(line);
        }

        public decimal TotalDebits => Lines.Sum(l => l.Debit);
        public decimal TotalCredits => Lines.Sum(l => l.Credit);
        public bool IsBalanced => Math.Round(TotalDebits - TotalCredits, 2) == 0m;

        private bool CanModify() => SelectedEntry != null;

        public partial class EditableLine : ObservableObject
        {
            [ObservableProperty]
            private int id;

            [ObservableProperty]
            private int chartOfAccountId;

            [ObservableProperty]
            private decimal debit;

            partial void OnDebitChanged(decimal value) => OnPropertyChanged(nameof(Amount));

            [ObservableProperty]
            private decimal credit;

            partial void OnCreditChanged(decimal value) => OnPropertyChanged(nameof(Amount));

            public decimal Amount => Debit - Credit;
        }
    }
}