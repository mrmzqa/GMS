using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfPosApp.Data;
using WpfPosApp.Services;
using WpfPosApp.ViewModels;
using WpfPosApp.Views;
using Microsoft.EntityFrameworkCore;

namespace WpfPosApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize database
            using var dbContext = new AppDbContext();
            dbContext.Database.Migrate();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>();
            
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IPosService, PosService>();
            
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<CartViewModel>();
            services.AddTransient<OrdersViewModel>();
            
            services.AddSingleton<MainWindow>(s => new MainWindow()
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });
            
            services.AddSingleton<LoginView>();
            services.AddSingleton<HomeView>();
            services.AddSingleton<ProductsView>();
            services.AddSingleton<CartView>();
            services.AddSingleton<OrdersView>();
        }
    }
}
