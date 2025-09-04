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
    public partial class AccountsPayableViewModel : ObservableObject
    {
        private readonly IRepository<AccountsPayable> _repo;

        public ObservableCollection<AccountsPayable> Payables { get; } = new();

        public AccountsPayableViewModel(IRepository<AccountsPayable> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _ = LoadAsync();
        }

        [ObservableProperty]
        private AccountsPayable? selectedPayable;

        partial void OnSelectedPayableChanged(AccountsPayable? value)
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
                Payables.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var p in list) Payables.Add(p);
                SelectedPayable = Payables.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load AP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public Task AddAsync()
        {
            var ap = new AccountsPayable
            {
                InvoiceDate = DateTime.UtcNow,
                InvoiceNumber = string.Empty,
                Amount = 0m,
                PaidAmount = 0m,
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Unpaid
            };
            Payables.Add(ap);
            SelectedPayable = ap;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedPayable == null) return;

            try
            {
                var detached = new AccountsPayable
                {
                    Id = SelectedPayable.Id,
                    VendorId = SelectedPayable.VendorId,
                    InvoiceDate = SelectedPayable.InvoiceDate,
                    InvoiceNumber = SelectedPayable.InvoiceNumber?.Trim() ?? string.Empty,
                    Amount = SelectedPayable.Amount,
                    PaidAmount = SelectedPayable.PaidAmount,
                    DueDate = SelectedPayable.DueDate,
                    Status = SelectedPayable.Status
                };

                if (detached.PaidAmount <= 0) detached.Status = InvoiceStatus.Unpaid;
                else if (detached.PaidAmount < detached.Amount) detached.Status = InvoiceStatus.PartiallyPaid;
                else detached.Status = InvoiceStatus.Paid;

                if (detached.Id == 0) await _repo.AddAsync(detached);
                else await _repo.UpdateAsync(detached);

                await LoadAsync();
                SelectedPayable = Payables.FirstOrDefault(p => p.InvoiceNumber == detached.InvoiceNumber) ?? SelectedPayable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save AP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedPayable == null) return;

            var confirm = MessageBox.Show("Delete selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (SelectedPayable.Id == 0)
                {
                    Payables.Remove(SelectedPayable);
                    SelectedPayable = Payables.FirstOrDefault();
                }
                else
                {
                    await _repo.DeleteAsync(SelectedPayable.Id);
                    await LoadAsync();
                    SelectedPayable = Payables.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete AP: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public decimal Balance => SelectedPayable == null ? 0m : Math.Round(SelectedPayable.Amount - SelectedPayable.PaidAmount, 2);

        private bool CanModify() => SelectedPayable != null;
    }
}