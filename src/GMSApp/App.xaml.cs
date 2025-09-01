using GmsApp.Views;
using GMSApp.Data;
using GMSApp.Repositories;
using GMSApp.ViewModels;
using GMSApp.ViewModels.Job;
using GMSApp.Views;
using GMSApp.Views.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Windows;
namespace GMSApp;

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
                // DbContext: keep your connection string here or use context.Configuration.GetConnectionString("DefaultConnection")
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GMSAppDb;Trusted_Connection=True;"));

                // Repositories
                services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                services.AddScoped<IFileRepository, FileRepository>();

                // Register PDF generator as transient (stateless)
                services.AddTransient(typeof(IGenericPdfGenerator<>), typeof(GenericPdfGenerator<>));
                // If you still want the generic registration for other types keep it, but ensure Joborder uses the concrete generator:
                services.AddTransient<IGenericPdfGenerator<GMSApp.Models.job.Joborder>, GMSApp.Repositories.Pdf.JoborderPdfGenerator>();
                // ViewModels - transient so UI gets fresh instances (avoids stale state)
                services.AddTransient<FileViewModel>();
                services.AddTransient<CoreMainViewModel>();
                services.AddTransient<JobContentViewModel>();
                
                services.AddTransient<JoborderViewModel>();

                // Views - transient
                services.AddTransient<MainWindow>();
                services.AddTransient<JobContentView>();
                services.AddTransient<PurchaseOrder>();
                services.AddTransient<JobOrder>();

                services.AddTransient<FilesPage>();
                services.AddTransient<CoreMain>();
            })
            .UseSerilog()
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        Log.Information("Application starting");

        try
        {
            await _host.StartAsync();

            // Apply EF migrations (preferred to EnsureCreated when using migrations)
            try
            {
                using var scope = _host.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception dbEx)
            {
                Log.Error(dbEx, "Database migration/initialization failed");
                MessageBox.Show($"Database initialization failed: {dbEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally rethrow or continue depending on your policy
            }

            // Show main window (resolve from DI so dependencies are injected)
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Host start failed");
            MessageBox.Show($"Application failed to start: {ex.Message}", "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            // If startup fails, shut down
            Shutdown();
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Application exiting");

        try
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error while stopping host");
        }
        finally
        {
            _host.Dispose();
            Log.CloseAndFlush();
        }

        base.OnExit(e);
    }
}
