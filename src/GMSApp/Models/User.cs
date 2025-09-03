using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } =  string.Empty ;


        public bool IsAuthenticated { get; set; }
    }
}
// Services/AuthService.cs
public class AuthService
{
    private readonly List<User> _users = new()
    {
        new User { Username = "admin", Password = "password123" },
        new User { Username = "user", Password = "user123" }
    };

    public User? CurrentUser { get; private set; }

    public bool Login(string username, string password)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username == username && u.Password == password);
        
        if (user != null)
        {
            CurrentUser = user;
            CurrentUser.IsAuthenticated = true;
            return true;
        }
        
        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public bool IsLoggedIn() => CurrentUser != null;
}
// ViewModels/MainViewModel.cs
public class MainViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoggedIn;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        _authService = new AuthService();
        LoginCommand = new DelegateCommand(ExecuteLogin, CanExecuteLogin);
        LogoutCommand = new DelegateCommand(ExecuteLogout, CanExecuteLogout);
    }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
            LoginCommand.RaiseCanExecuteChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            LoginCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            OnPropertyChanged();
            LoginCommand.RaiseCanExecuteChanged();
            LogoutCommand.RaiseCanExecuteChanged();
        }
    }

    public DelegateCommand LoginCommand { get; }
    public DelegateCommand LogoutCommand { get; }

    private void ExecuteLogin(object? parameter)
    {
        if (_authService.Login(Username, Password))
        {
            IsLoggedIn = true;
            StatusMessage = $"Welcome, {Username}!";
            Password = string.Empty; // Clear password after login
        }
        else
        {
            StatusMessage = "Invalid username or password!";
        }
    }

    private bool CanExecuteLogin(object? parameter)
    {
        return !IsLoggedIn && 
               !string.IsNullOrWhiteSpace(Username) && 
               !string.IsNullOrWhiteSpace(Password);
    }

    private void ExecuteLogout(object? parameter)
    {
        _authService.Logout();
        IsLoggedIn = false;
        Username = string.Empty;
        Password = string.Empty;
        StatusMessage = "Logged out successfully!";
    }

    private bool CanExecuteLogout(object? parameter)
    {
        return IsLoggedIn;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
// Commands/DelegateCommand.cs
public class DelegateCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
<!-- MainWindow.xaml -->
<Window x:Class="WpfLoginApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Login System" Height="400" Width="400"
        WindowStartupLocation="CenterScreen">
    
    <Window.Resources>
        <Style TargetType="TextBox">
            <Setter Property="Margin" Value="10" />
            <Setter Property="Padding" Value="5" />
        </Style>
        
        <Style TargetType="PasswordBox">
            <Setter Property="Margin" Value="10" />
            <Setter Property="Padding" Value="5" />
        </Style>
        
        <Style TargetType="Button">
            <Setter Property="Margin" Value="10" />
            <Setter Property="Padding" Value="10,5" />
            <Setter Property="MinWidth" Value="80" />
        </Style>
        
        <Style TargetType="TextBlock">
            <Setter Property="Margin" Value="10" />
            <Setter Property="VerticalAlignment" Value="Center" />
        </Style>
    </Window.Resources>

    <Grid>
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center" Width="300">
            
            <!-- Login Section -->
            <StackPanel Visibility="{Binding IsLoggedIn, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=reverse}">
                <TextBlock Text="Username:" FontWeight="Bold" />
                <TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" />
                
                <TextBlock Text="Password:" FontWeight="Bold" Margin="0,10,0,0" />
                <PasswordBox x:Name="PasswordBox" PasswordChanged="PasswordBox_PasswordChanged" />
                
                <Button Content="Login" 
                        Command="{Binding LoginCommand}"
                        IsDefault="True"
                        HorizontalAlignment="Right" />
            </StackPanel>

            <!-- Logout Section -->
            <StackPanel Visibility="{Binding IsLoggedIn, Converter={StaticResource BooleanToVisibilityConverter}}"
                        HorizontalAlignment="Center">
                <TextBlock Text="{Binding StatusMessage}" 
                           FontSize="16" 
                           Foreground="Green"
                           TextAlignment="Center"
                           Margin="0,20" />
                
                <Button Content="Logout" 
                        Command="{Binding LogoutCommand}"
                        HorizontalAlignment="Center" />
            </StackPanel>

            <!-- Status Message -->
            <TextBlock Text="{Binding StatusMessage}" 
                       Foreground="Red"
                       TextAlignment="Center"
                       Margin="0,20"
                       Visibility="{Binding StatusMessage, Converter={StaticResource StringToVisibilityConverter}}" />
        </StackPanel>
    </Grid>
</Window>
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}

// Converters/BooleanToVisibilityConverter.cs
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // If parameter is "reverse", invert the logic
            if (parameter?.ToString()?.ToLower() == "reverse")
                boolValue = !boolValue;
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converters/StringToVisibilityConverter.cs
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
<!-- App.xaml -->
<Application x:Class="WpfLoginApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:WpfLoginApp"
             StartupUri="MainWindow.xaml">
    
    <Application.Resources>
        <ResourceDictionary>
            <local:BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
            <local:StringToVisibilityConverter x:Key="StringToVisibilityConverter" />
        </ResourceDictionary>
    </Application.Resources>
</Application>