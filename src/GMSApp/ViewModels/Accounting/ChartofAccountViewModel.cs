
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

        partial void OnSelectedAccountChanged(ChartOfAccount? value)
        {
            // ensure parent reference exists in collection for binding
            OnPropertyChanged(nameof(Accounts));
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
                Accounts.Clear();
                var list = await _repo.GetAllAsync();
                foreach (var a in list)
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
            var acc = new ChartOfAccount
            {
                AccountCode = string.Empty,
                AccountName = string.Empty,
                AccountType = AccountType.Asset,
                IsActive = true
            };
            Accounts.Add(acc);
            SelectedAccount = acc;
            return Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task SaveAsync()
        {
            if (SelectedAccount == null) return;

            try
            {
                // Clean navigation properties
                SelectedAccount.ParentAccount = null;
                SelectedAccount.SubAccounts = null!;

                // Set ParentAccountId to null if ParentAccount is null
                if (SelectedAccount.ParentAccount == null)
                {
                    SelectedAccount.ParentAccountId = null;
                }
                else
                {
                    SelectedAccount.ParentAccountId = SelectedAccount.ParentAccount?.Id;
                }

                // Save or update the selected account
                if (SelectedAccount.Id == 0)
                {
                    await _repo.AddAsync(SelectedAccount);
                }
                else
                {
                    await _repo.UpdateAsync(SelectedAccount);
                }

                // Reload accounts after save
                await LoadAsync();
                SelectedAccount = Accounts.FirstOrDefault(a => a.Id == SelectedAccount.Id) ?? SelectedAccount;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save account:\n {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [RelayCommand(CanExecute = nameof(CanModify))]
        public async Task DeleteAsync()
        {
            if (SelectedAccount == null) return;

            var confirm = MessageBox.Show("Delete selected account?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

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
                    SelectedAccount = Accounts.FirstOrDefault();
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