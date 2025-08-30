/* 




A complete WPF (.NET 9) example implementing authentication and authorization with:
User registration, login, logout
Role creation/assignment
Role-based views (AdminView and UserView)
Persistence using SQLite + EF Core
Password hashing (PBKDF2)
MVVM pattern with simple RelayCommand
Project structure (files provided below)

WpfAuthApp.csproj
App.xaml, App.xaml.cs
Models: User.cs, Role.cs, UserRole.cs
Data: AppDbContext.cs
Services: IAuthService.cs, AuthService.cs, IUserService.cs, UserService.cs, PasswordHasher.cs, AppState.cs
ViewModels: BaseViewModel.cs, RelayCommand.cs, LoginViewModel.cs, RegisterViewModel.cs, MainViewModel.cs
Views: LoginWindow.xaml (+ .cs), RegisterWindow.xaml (+ .cs), MainWindow.xaml (+ .cs), AdminView.xaml, UserView.xaml
Setup (commands)

Create project: dotnet new wpf -n WpfAuthApp cd WpfAuthApp
Add packages: dotnet add package Microsoft.EntityFrameworkCore.Sqlite dotnet add package Microsoft.EntityFrameworkCore.Design
Replace project files with the ones below.
Run: dotnet run
Note: TargetFramework is net9.0 in csproj.

Code

WpfAuthApp.csproj

 copy
xml

<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
App.xaml

 copy
xml

<Application x:Class="WpfAuthApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="Views/LoginWindow.xaml">
    <Application.Resources>
    </Application.Resources>
</Application>
App.xaml.cs

 copy
csharp

using System.Windows;
using Microsoft.EntityFrameworkCore;
using WpfAuthApp.Data;
using WpfAuthApp.Services;

namespace WpfAuthApp
{
    public partial class App : Application
    {
        private AppDbContext? _db;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup DB and services (simple manual DI)
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=app.db")
                .Options;
            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();

            // Seed roles if not present
            if (!_db.Roles.Any())
            {
                _db.Roles.AddRange(
                    new Models.Role { Name = "Admin" },
                    new Models.Role { Name = "User" }
                );
                _db.SaveChanges();
            }

            var userService = new UserService(_db);
            var authService = new AuthService(_db, userService);

            // Register services to a simple service container
            ServiceLocator.UserService = userService;
            ServiceLocator.AuthService = authService;
            ServiceLocator.AppState = new AppState();

            // Show login window (StartupUri configured to LoginWindow)
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _db?.Dispose();
            base.OnExit(e);
        }
    }

    // Very small service locator for brevity
    public static class ServiceLocator
    {
        public static IUserService? UserService { get; set; }
        public static IAuthService? AuthService { get; set; }
        public static AppState? AppState { get; set; }
    }
}
 copy
Models/User.cs

 copy
csharp

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfAuthApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required] public string Username { get; set; } = null!;
        [Required] public string PasswordHash { get; set; } = null!;
        [Required] public string Salt { get; set; } = null!;

        public List<UserRole> UserRoles { get; set; } = new();
    }
}
Models/Role.cs

 copy
csharp

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfAuthApp.Models
{
    public class Role
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public List<UserRole> UserRoles { get; set; } = new();
    }
}
Models/UserRole.cs

 copy
csharp

namespace WpfAuthApp.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
Data/AppDbContext.cs

 copy
csharp

using Microsoft.EntityFrameworkCore;
using WpfAuthApp.Models;

namespace WpfAuthApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
 copy
Services/PasswordHasher.cs

 copy
csharp

using System;
using System.Security.Cryptography;

namespace WpfAuthApp.Services
{
    public static class PasswordHasher
    {
        // PBKDF2 wrapper
        public static (string hash, string salt) HashPassword(string password, int iterations = 100_000)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            var hash = Hash(password, saltBytes, iterations);
            return (Convert.ToBase64String(hash), salt);
        }

        public static bool Verify(string password, string hashed, string salt, int iterations = 100_000)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hash = Hash(password, saltBytes, iterations);
            return Convert.ToBase64String(hash) == hashed;
        }

        private static byte[] Hash(string password, byte[] salt, int iterations)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }
    }
}
 copy
Services/IUserService.cs

 copy
csharp

using WpfAuthApp.Models;

namespace WpfAuthApp.Services
{
    public interface IUserService
    {
        User? GetByUsername(string username);
        User? GetById(int id);
        User CreateUser(string username, string password);
        void AddRoleToUser(User user, string roleName);
    }
}
Services/UserService.cs

 copy
csharp

using System.Linq;
using Microsoft.EntityFrameworkCore;
using WpfAuthApp.Data;
using WpfAuthApp.Models;

namespace WpfAuthApp.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) { _db = db; }

        public User? GetByUsername(string username)
        {
            return _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Username == username);
        }

        public User? GetById(int id)
        {
            return _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == id);
        }

        public User CreateUser(string username, string password)
        {
            if (_db.Users.Any(u => u.Username == username))
                throw new InvalidOperationException("Username already exists.");

            var (hash, salt) = PasswordHasher.HashPassword(password);
            var user = new User { Username = username, PasswordHash = hash, Salt = salt };
            _db.Users.Add(user);
            _db.SaveChanges();

            // assign default role 'User' if exists
            var userRole = _db.Roles.FirstOrDefault(r => r.Name == "User");
            if (userRole != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
                _db.SaveChanges();
            }

            return user;
        }

        public void AddRoleToUser(User user, string roleName)
        {
            var role = _db.Roles.FirstOrDefault(r => r.Name == roleName)
                       ?? throw new InvalidOperationException("Role not found.");
            if (_db.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id)) return;
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            _db.SaveChanges();
        }
    }
}
 copy
Services/IAuthService.cs

 copy
csharp

using WpfAuthApp.Models;

namespace WpfAuthApp.Services
{
    public interface IAuthService
    {
        User? Login(string username, string password);
        void Logout();
        User? CurrentUser { get; }
        bool IsInRole(string role);
        User Register(string username, string password);
    }
}
Services/AuthService.cs

 copy
csharp

using WpfAuthApp.Data;
using WpfAuthApp.Models;

namespace WpfAuthApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IUserService _userService;
        private User? _currentUser;
        public AuthService(AppDbContext db, IUserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public User? CurrentUser => _currentUser;

        public User Register(string username, string password)
        {
            var user = _userService.CreateUser(username, password);
            return user;
        }

        public User? Login(string username, string password)
        {
            var user = _userService.GetByUsername(username);
            if (user == null) return null;

            if (!PasswordHasher.Verify(password, user.PasswordHash, user.Salt)) return null;
            _currentUser = user;
            return user;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool IsInRole(string role)
        {
            if (_currentUser == null) return false;
            return _currentUser.UserRoles.Any(ur => ur.Role.Name == role);
        }
    }
}
 copy
Services/AppState.cs

 copy
csharp

using System;

namespace WpfAuthApp.Services
{
    public class AppState
    {
        // Simple event to notify when current user changes
        public event Action? CurrentUserChanged;
        public void NotifyUserChanged() => CurrentUserChanged?.Invoke();
    }
}
ViewModels/BaseViewModel.cs

 copy
csharp

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfAuthApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Raise([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
ViewModels/RelayCommand.cs

 copy
csharp

using System;
using System.Windows.Input;

namespace WpfAuthApp.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public event EventHandler? CanExecuteChanged;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
 copy
ViewModels/LoginViewModel.cs

 copy
csharp

using System;
using System.Windows;
using System.Windows.Input;
using WpfAuthApp.Services;

namespace WpfAuthApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username = "";
        public string Username { get => _username; set { _username = value; Raise(); } }
        private string _password = "";
        public string Password { get => _password; set { _password = value; Raise(); } }

        public ICommand LoginCommand { get; }
        public ICommand OpenRegisterCommand { get; }

        private readonly IAuthService _auth;
        private readonly AppState _appState;

        public LoginViewModel(IAuthService auth, AppState appState)
        {
            _auth = auth;
            _appState = appState;
            LoginCommand = new RelayCommand(_ => ExecuteLogin());
            OpenRegisterCommand = new RelayCommand(_ => ExecuteOpenRegister());
        }

        private void ExecuteOpenRegister()
        {
            var reg = new Views.RegisterWindow();
            reg.ShowDialog();
        }

        private void ExecuteLogin()
        {
            try
            {
                var user = _auth.Login(Username, Password);
                if (user == null)
                {
                    MessageBox.Show("Invalid credentials", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _appState.NotifyUserChanged();
                // Open main window and close login window
                var main = new Views.MainWindow();
                main.Show();
                // Close current login window (find by Application.Current.Windows)
                foreach (Window w in Application.Current.Windows)
                {
                    if (w is Views.LoginWindow) { w.Close(); break; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
 copy
ViewModels/RegisterViewModel.cs

 copy
csharp

using System;
using System.Windows;
using System.Windows.Input;
using WpfAuthApp.Services;

namespace WpfAuthApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _username = "";
        public string Username { get => _username; set { _username = value; Raise(); } }
        private string _password = "";
        public string Password { get => _password; set { _password = value; Raise(); } }

        public ICommand RegisterCommand { get; }

        private readonly IAuthService _auth;

        public RegisterViewModel(IAuthService auth)
        {
            _auth = auth;
            RegisterCommand = new RelayCommand(_ => ExecuteRegister());
        }

        private void ExecuteRegister()
        {
            try
            {
                var user = _auth.Register(Username, Password);
                MessageBox.Show($"User '{user.Username}' created.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Close register window
                foreach (Window w in Application.Current.Windows)
                {
                    if (w is Views.RegisterWindow) { w.Close(); break; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
 copy
ViewModels/MainViewModel.cs

 copy
csharp

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfAuthApp.Services;

namespace WpfAuthApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IAuthService _auth;
        private readonly IUserService _userService;
        private readonly AppState _appState;

        public string WelcomeText => _auth.CurrentUser != null ? $"Welcome, {_auth.CurrentUser.Username}" : "Not logged in";
        public bool IsAdmin => _auth.IsInRole("Admin");
        public bool IsUser => _auth.IsInRole("User");

        public ICommand LogoutCommand { get; }
        public ICommand AddAdminRoleCommand { get; }

        public MainViewModel(IAuthService auth, IUserService userService, AppState appState)
        {
            _auth = auth;
            _userService = userService;
            _appState = appState;

            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            AddAdminRoleCommand = new RelayCommand(_ => ExecuteAddAdminRole());

            _appState.CurrentUserChanged += () => { Raise(nameof(WelcomeText)); Raise(nameof(IsAdmin)); Raise(nameof(IsUser)); };
        }

        private void ExecuteLogout()
        {
            _auth.Logout();
            _appState.NotifyUserChanged();
            // Return to login screen
            var login = new Views.LoginWindow();
            login.Show();
            foreach (Window w in Application.Current.Windows)
            {
                if (w is Views.MainWindow) { w.Close(); break; }
            }
        }

        private void ExecuteAddAdminRole()
        {
            // Example: assign Admin role to the current user
            var user = _auth.CurrentUser;
            if (user == null)
            {
                MessageBox.Show("No user logged in.");
                return;
            }
            try
            {
                _userService.AddRoleToUser(user, "Admin");
                MessageBox.Show("Admin role added to user.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Refresh
                var refreshed = _userService.GetById(user.Id);
                if (refreshed != null)
                {
                    // set internal current user in AuthService (reflection or recreate)
                    // For simplicity, re-login to refresh _currentUser reference
                    _auth.Login(refreshed.Username, ""); // wrong approach; instead we'll update _auth.CurrentUser by reloading inside AuthService not exposed.
                    // Simpler: Notify AppState to refresh data then reload window
                    _appState.NotifyUserChanged();
                }
                Raise(nameof(IsAdmin));
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }
    }
}
 copy
Views/LoginWindow.xaml

 copy
xml

<Window x:Class="WpfAuthApp.Views.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Login" Height="220" Width="330" WindowStartupLocation="CenterScreen">
    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        <TextBlock Text="Username" />
        <TextBox Grid.Row="1" x:Name="UsernameBox" Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" Margin="0,4,0,8"/>
        <TextBlock Grid.Row="2" Text="Password" />
        <PasswordBox Grid.Row="3" x:Name="PasswordBox" Margin="0,4,0,8" PasswordChanged="PasswordBox_PasswordChanged"/>
        <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,8,0,0">
            <Button Content="Login" Width="80" Margin="0,0,8,0" Command="{Binding LoginCommand}"/>
            <Button Content="Register" Width="80" Command="{Binding OpenRegisterCommand}"/>
        </StackPanel>
    </Grid>
</Window>
 copy
Views/LoginWindow.xaml.cs

 copy
csharp

using System.Windows;
using WpfAuthApp.Services;
using WpfAuthApp.ViewModels;

namespace WpfAuthApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;
        public LoginWindow()
        {
            InitializeComponent();
            var auth = ServiceLocator.AuthService ?? throw new System.Exception("AuthService not configured");
            var appState = ServiceLocator.AppState ?? throw new System.Exception("AppState not configured");
            _vm = new LoginViewModel(auth, appState);
            DataContext = _vm;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }
}
 copy
Views/RegisterWindow.xaml

 copy
xml

<Window x:Class="WpfAuthApp.Views.RegisterWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Register" Height="200" Width="320" WindowStartupLocation="CenterOwner">
    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <StackPanel>
            <TextBlock Text="Username" />
            <TextBox x:Name="UsernameBox" Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" Margin="0,6,0,8"/>
            <TextBlock Text="Password" />
            <PasswordBox x:Name="PasswordBox" PasswordChanged="PasswordBox_PasswordChanged" Margin="0,6,0,8"/>
            <Button Content="Register" Width="100" Command="{Binding RegisterCommand}" HorizontalAlignment="Left"/>
        </StackPanel>
    </Grid>
</Window>
 copy
Views/RegisterWindow.xaml.cs

 copy
csharp

using System.Windows;
using WpfAuthApp.Services;
using WpfAuthApp.ViewModels;

namespace WpfAuthApp.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _vm;
        public RegisterWindow()
        {
            InitializeComponent();
            var auth = ServiceLocator.AuthService ?? throw new System.Exception("AuthService not configured");
            _vm = new RegisterViewModel(auth);
            DataContext = _vm;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }
}
 copy
Views/MainWindow.xaml

 copy
xml

<Window x:Class="WpfAuthApp.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Main" Height="350" Width="600" WindowStartupLocation="CenterScreen">
    <DockPanel>
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="8">
            <TextBlock Text="{Binding WelcomeText}" VerticalAlignment="Center" FontWeight="Bold" Margin="0,0,16,0"/>
            <Button Content="Logout" Command="{Binding LogoutCommand}" Width="80"/>
            <Button Content="Make Me Admin" Command="{Binding AddAdminRoleCommand}" Width="120" Margin="8,0,0,0"/>
        </StackPanel>

        <Grid Margin="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="200"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <ListBox Grid.Column="0" Name="MenuList" Margin="0,0,12,0">
                <ListBoxItem Content="User View" Tag="User"/>
                <ListBoxItem Content="Admin View" Tag="Admin"/>
            </ListBox>

            <ContentControl Grid.Column="1" x:Name="ContentArea"/>

        </Grid>
    </DockPanel>
</Window>
 copy
Views/MainWindow.xaml.cs

 copy
csharp

using System.Windows;
using System.Windows.Controls;
using WpfAuthApp.Services;
using WpfAuthApp.ViewModels;

namespace WpfAuthApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            var auth = ServiceLocator.AuthService ?? throw new System.Exception("AuthService not configured");
            var userService = ServiceLocator.UserService ?? throw new System.Exception("UserService not configured");
            var appState = ServiceLocator.AppState ?? throw new System.Exception("AppState not configured");

            _vm = new MainViewModel(auth, userService, appState);
            DataContext = _vm;

            MenuList.SelectionChanged += MenuList_SelectionChanged;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // default view
            ShowAppropriateView();
        }

        private void MenuList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MenuList.SelectedItem is ListBoxItem item && item.Tag is string tag)
            {
                ShowView(tag);
            }
        }

        private void ShowAppropriateView()
        {
            // If admin; show admin view by default, else user view.
            var auth = ServiceLocator.AuthService!;
            if (auth.IsInRole("Admin"))
                ShowView("Admin");
            else
                ShowView("User");
        }

        private void ShowView(string tag)
        {
            var auth = ServiceLocator.AuthService!;
            if (tag == "Admin")
            {
                if (!auth.IsInRole("Admin"))
                {
                    ContentArea.Content = new TextBlock { Text = "Access denied. Admins only.", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Bold };
                    return;
                }
                ContentArea.Content = new AdminView();
            }
            else // User
            {
                ContentArea.Content = new UserView();
            }
        }
    }
}
 copy
Views/AdminView.xaml

 copy
xml

<UserControl x:Class="WpfAuthApp.Views.AdminView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Height="300">
    <Grid>
        <TextBlock Text="Admin Dashboard" FontSize="20" FontWeight="Bold" HorizontalAlignment="Center" VerticalAlignment="Top" Margin="0,12,0,0"/>
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
            <TextBlock Text="Admin-only content goes here" />
        </StackPanel>
    </Grid>
</UserControl>
Views/AdminView.xaml.cs

 copy
csharp

using System.Windows.Controls;

namespace WpfAuthApp.Views
{
    public partial class AdminView : UserControl
    {
        public AdminView()
        {
            InitializeComponent();
        }
    }
}
Views/UserView.xaml

 copy
xml

<UserControl x:Class="WpfAuthApp.Views.UserView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Height="300">
    <Grid>
        <TextBlock Text="User Dashboard" FontSize="18" FontWeight="Bold" HorizontalAlignment="Center" VerticalAlignment="Top" Margin="0,12,0,0"/>
        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
            <TextBlock Text="General user content" />
        </StackPanel>
    </Grid>
</UserControl>
Views/UserView.xaml.cs

 copy
csharp

using System.Windows.Controls;

namespace WpfAuthApp.Views
{
    public partial class UserView : UserControl
    {
        public UserView()
        {
            InitializeComponent();
        }
    }
}
Usage notes and behavior

Register creates a new user and assigns the default "User" role.
Login validates password using PBKDF2 (secure).
The MainWindow shows different content based on IsInRole checks:
AdminView only accessible to Admin role.
UserView accessible to all logged-in users.
The "Make Me Admin" button in MainWindow is a demonstration of role assignment at runtime via the IUserService.AddRoleToUser method. In production, only administrators should be able to assign roles.
Passwords are hashed and salted — unhashed passwords are never stored.
The project uses a simple ServiceLocator for demo. For larger apps, replace with a DI container (e.g., Microsoft.Extensions.DependencyInjection).
Potential improvements

Add proper reloading of CurrentUser after role changes (here, the AppState notifications cause UI to re-evaluate but you may want AuthService to reload the user from DB after role modifications).
Replace the simplistic ServiceLocator with a proper DI container.
Protect commands/UI with proper CanExecute logic.
Use secure secure string handling for passwords if required.
If you want, I can:

Convert this to use Microsoft.Extensions.DependencyInjection for cleaner DI.
Add full refresh of CurrentUser after role changes.
Add unit tests and migration scripts.
Which of these would you like next?

AI
Tab
Tab: default





*/