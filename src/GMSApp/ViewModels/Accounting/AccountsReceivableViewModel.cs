using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Commands;
using GMSApp.Models;
using GMSApp.Models.account;
using GMSApp.Models.invoice;
using GMSApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels.Accounting
{
    public partial class AccountsReceivableViewModel : ObservableObject
    {
        private readonly IRepository<AccountsReceivable> _repo;

        public ObservableCollection<AccountsReceivable> Receivables { get; } = new();

        public AccountsReceivableViewModel(IRepository<AccountsReceivable> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private AccountsReceivable? selectedReceivable;

        partial void OnSelectedReceivableChanged(AccountsReceivable? value)
        {
            OnPropertyChanged(nameof(Balance));
            NotifyCommands();
        }

        private void NotifyCommands()
        {
            LoadCommand.NotifyCanExecuteChanged();
            AddCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                Receivables.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var r in list) Receivables.Add(r);
                SelectedReceivable = Receivables.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load AR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var ar = new AccountsReceivable
            {
                InvoiceDate = DateTime.UtcNow,
                InvoiceNumber = string.Empty,
                Amount = 0m,
                ReceivedAmount = 0m,
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Draft
            };
            Receivables.Add(ar);
            SelectedReceivable = ar;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedReceivable == null) return;

            try
            {
                // create detached copy
                var detached = new AccountsReceivable
                {
                    Id = SelectedReceivable.Id,
                    CustomerId = SelectedReceivable.CustomerId,
                    InvoiceDate = SelectedReceivable.InvoiceDate,
                    InvoiceNumber = SelectedReceivable.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedReceivable.Amount,
                    ReceivedAmount = SelectedReceivable.ReceivedAmount,
                    DueDate = SelectedReceivable.DueDate,
                    Status = SelectedReceivable.Status
                };

                // update status based on amounts
                if (detached.ReceivedAmount <= 0) detached.Status = InvoiceStatus.Draft;
                else if (detached.ReceivedAmount < detached.Amount) detached.Status = InvoiceStatus.PartiallyPaid;
                else detached.Status = InvoiceStatus.Paid;

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedReceivable = Receivables.FirstOrDefault(r => r.InvoiceNumber == detached.InvoiceNumber) ?? SelectedReceivable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save AR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedReceivable == null) return;

            var confirm = MessageBox.Show("Delete selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedReceivable.Id == 0)
                {
                    Receivables.Remove(SelectedReceivable);
                    SelectedReceivable = Receivables.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedReceivable.Id);
                    await LoadAsync();
                    SelectedReceivable = Receivables.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete AR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public decimal Balance => SelectedReceivable == null ? 0m : Math.Round(SelectedReceivable.Amount - SelectedReceivable.ReceivedAmount, 2);

        private bool CanModify() => SelectedReceivable != null;
    }
}