using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static MaterialDesignThemes.Wpf.Theme;

namespace GMSApp.Views
{
    /// <summary>
    /// Interaction logic for UI.xaml
    /// </summary>
    public partial class UI : Window
    {
        public UI()
        {
            InitializeComponent();

            InitializeComponent();

            InitializeComponent();

            var table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Email");

            table.Rows.Add("Alice Johnson", "alice@example.com");
            table.Rows.Add("Bob Smith", "bob@example.com");

            // Bind to DataGrid
            UserDataGrid.ItemsSource = table.DefaultView;
        }
    }
}

