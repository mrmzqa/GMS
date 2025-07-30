// ViewModels/ServiceJobViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GMSApp.ViewModels
{
    public partial class ServiceJobViewModel : BaseViewModel
    {
        private readonly GarageDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<ServiceJob> serviceJobs = new();

        [ObservableProperty]
        private ServiceJob? selectedServiceJob;

        public ObservableCollection<GarageWorker> Workers { get; } = new();

        public ServiceJobViewModel(GarageDbContext dbContext)
        {
            _dbContext = dbContext;
            LoadDataAsync();
        }

        async Task LoadDataAsync()
        {
            var jobs = await _dbContext.ServiceJobs
                .Include(j => j.Vehicle)
                .Include(j => j.AssignedWorker)
                .ToListAsync();

            serviceJobs = new ObservableCollection<ServiceJob>(jobs);

            var workers = await _dbContext.GarageWorkers.ToListAsync();
            Workers.Clear();

            foreach(var w in workers)
                Workers.Add(w);
        }

        [ICommand]
        public async Task AddServiceJobAsync()
        {
            var job = new ServiceJob
            {
                ReportedIssue = "New issue",
                StartDate = DateTime.UtcNow,
                Cost = 0m
            };

            _dbContext.ServiceJobs.Add(job);
            await _dbContext.SaveChangesAsync();

            ServiceJobs.Add(job);
            SelectedServiceJob = job;
        }

        [ICommand(CanExecute = nameof(CanEditOrDelete))]
        public async Task DeleteServiceJobAsync()
        {
            if (SelectedServiceJob == null) return;

            _dbContext.ServiceJobs.Remove(SelectedServiceJob);
            await _dbContext.SaveChangesAsync();

            ServiceJobs.Remove(SelectedServiceJob);
            SelectedServiceJob = null;
        }

        private bool CanEditOrDelete() => SelectedServiceJob != null;

        [ICommand(CanExecute = nameof(CanEditOrDelete))]
        public async Task UpdateServiceJobAsync()
        {
            if(SelectedServiceJob != null)
            {
                _dbContext.ServiceJobs.Update(SelectedServiceJob);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}