using GmsApp.Views;
using GMSApp.Data;
using GMSApp.Repositories;
using GMSApp.ViewModels;
using GMSApp.Views;
using GMSApp.Views.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Windows;

namespace GMSApp
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\Appdb.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // DbContext
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GMSAppDb;Trusted_Connection=True;"));
                    /*services.AddDbContext<AppDbContext>(options =>
                                 options.UseSqlite("Data Source=Appdb.db"));*/
                    // Repositories
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IFileRepository, FileRepository>();

                    // ViewModels
                    services.AddScoped<FileViewModel>();

                    // Views
                    services.AddScoped<MainWindow>();


                   
                    services.AddScoped<MainContentView>();
                    services.AddScoped<PurchaseOrderPage>();
                    services.AddScoped<FilesPage>();
                     services.AddScoped<CoreMainViewModel>();
                    services.AddScoped<MainContentViewModel>();
                    services.AddScoped<PurchaseOrderViewModel>();
                    services.AddScoped<CoreMain>();

                   
                })




                .UseSerilog()
                .Build();
        }

       /* protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Information("Application starting");

            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();

            mainWindow.Show();

            base.OnStartup(e);
        }*/
        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Information("Application starting");

            await _host.StartAsync();

            var dbContext = _host.Services.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Log.Information("Application exiting");

            await _host.StopAsync();
            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}
