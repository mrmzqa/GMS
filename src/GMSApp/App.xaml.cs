using GMSApp.Data;
using GMSApp.Repositories;
using GMSApp.Services;
using GMSApp.ViewModels;
using GMSApp.Views;
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
                .WriteTo.File("logs\\garage.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<GarageDbContext>(options =>
                        options.UseSqlite("Data Source=garage.db"));
                   

                   
                    services.AddSingleton<MainWindow>();
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddTransient<VehicleViewModel>();
                    services.AddTransient<VehiclesPage>();


                   

                })
                .UseSerilog()
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Information("Application starting");

            await _host.StartAsync();

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