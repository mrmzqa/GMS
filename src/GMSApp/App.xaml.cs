
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

public partial class App : Application
{
    /*public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer("YourConnectionStringHere"));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        ServiceProvider = services.BuildServiceProvider();

        base.OnStartup(e);
    }*/
}