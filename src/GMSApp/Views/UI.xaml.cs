/*using System;
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

Perfect — you want your reusable WPF dynamic form module to support bilingual UI (e.g., English + Arabic or any two languages).


---

✅ Goal: Add Bilingual Support to Your Reusable WPF Dynamic Form Module

We will implement:

✅ Features:

🔄 Switch language at runtime (e.g., using a ComboBox or Toggle)

🌐 Resource-based localization using .resx files

🔤 All labels/texts localized (Order Number, Add Row, etc.)

🚀 Reusable for all future modules

🕌 Arabic/RTL support (Right-to-Left layout)



---

🔧 Step-by-Step Implementation


---

🧩 1. Create Resource Files

In your Resources/ folder (add one if not exists), create:

File Name	Language

Strings.resx	Default (English)
Strings.ar.resx	Arabic


✅ Add keys like:

Key	Value (English)	Value (Arabic)

OrderNumberLabel	Order Number	رقم الطلب
AddItem	Add Item	إضافة عنصر
Save	Save	حفظ
ExportPdf	Export PDF	تصدير PDF
Total	Total	الإجمالي



---

🔁 2. Localization Helper – LocalizationService.cs

using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;

public static class LocalizationService
{
    private static ResourceManager _resourceManager = new("YourNamespace.Resources.Strings", typeof(LocalizationService).Assembly);

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


---

📜 3. Update XAML to Use Localized Text

<Window ...
        xmlns:local="clr-namespace:YourNamespace.Resources"
        x:Class="YourNamespace.PurchaseOrderForm"
        Title="{x:Static local:Strings.OrderNumberLabel}">

    <StackPanel Margin="10">
        <ComboBox Width="150" Margin="0,0,0,10"
                  SelectedValue="{Binding SelectedLanguage, Mode=TwoWay}">
            <ComboBoxItem Content="English" Tag="en" />
            <ComboBoxItem Content="العربية" Tag="ar" />
        </ComboBox>

        <TextBlock Text="{x:Static local:Strings.OrderNumberLabel}" />
        <TextBox Text="{Binding OrderNumber}" Width="200" />

        <!-- ... -->

        <Button Content="{x:Static local:Strings.AddItem}" Command="{Binding AddItemCommand}" Width="100" />
        <Button Content="{x:Static local:Strings.Save}" Command="{Binding SaveCommand}" Width="100" />
        <Button Content="{x:Static local:Strings.ExportPdf}" Command="{Binding ExportPdfCommand}" Width="100" />
    </StackPanel>
</Window>


---

🧠 4. ViewModel Language Binding

In PurchaseOrderViewModel.cs:

[ObservableProperty]
private string selectedLanguage = "en";

partial void OnSelectedLanguageChanged(string value)
{
    LocalizationService.SetCulture(value);
}


---

🕌 5. Arabic RTL UI Support

This line will switch flow automatically:

Application.Current.MainWindow.FlowDirection = cultureCode.StartsWith("ar") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

You can also apply FlowDirection="{Binding IsArabic, Converter=...}" at control level if needed.


---

🎁 Optional: Language Persistence

Save selected language in Properties.Settings.Default and load it at app start:

LocalizationService.SetCulture(Properties.Settings.Default.Language ?? "en");


---

🧩 Summary of Added Files

📁 Resources/
│   ├── Strings.resx       // English
│   └── Strings.ar.resx    // Arabic
📁 Services/
│   └── LocalizationService.cs


---

✅ Now Your Module Supports:

🗣 Bilingual UI (English/Arabic or more)

🔄 Runtime switching

🕌 RTL layout for Arabic

♻️ Reusable localization framework



---

Would you like me to generate the entire project structure as a .zip or a GitHub-ready repo with everything included (PDF, DB, bilingual, validation, dynamic rows)?

*/