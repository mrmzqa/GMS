namespace GMSApp.Services;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;

public static class LocalizationService
{
    private static ResourceManager _resourceManager = new("GMSApp.Resources.Strings", typeof(LocalizationService).Assembly);

    public static string Get(string key) => _resourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture) ?? key;

    public static void SetCulture(string cultureCode)
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
        Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode);

        foreach (Window window in Application.Current.Windows)
        {
            var oldContent = window.Content;
            window.Content = null;
            window.Content = oldContent; // Reloads bindings
        }

        FlowDirection flow = cultureCode.StartsWith("ar") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        Application.Current.MainWindow.FlowDirection = flow;
    }
}
