using System.Windows;
using System.Windows.Controls;

namespace Gms25.Views
{
    public partial class MainWindow : Window
    {
        public bool IsLoginPopupOpen
        {
            get { return (bool)GetValue(IsLoginPopupOpenProperty); }
            set { SetValue(IsLoginPopupOpenProperty, value); }
        }

        public static readonly DependencyProperty IsLoginPopupOpenProperty =
            DependencyProperty.Register("IsLoginPopupOpen", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsLoginPopupOpen = true;
        }

        private void CancelLogin_Click(object sender, RoutedEventArgs e)
        {
            IsLoginPopupOpen = false;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Add your login logic here
            IsLoginPopupOpen = false;
        }
    }
}