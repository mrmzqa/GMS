using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using static System.Net.Mime.MediaTypeNames;

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





/*< Window x: Class = "GMSApp.Views.MainWindow"
        xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml"
        Title = "Garage Management System"
        Height = "600" Width = "1000"
        WindowStartupLocation = "CenterScreen" >

    < DockPanel >
        < !--Top Menu-- >
        < Menu DockPanel.Dock = "Top" >
            < MenuItem Header = "File" >
                < MenuItem Header = "Login" Click = "FileOpen_Click" />
                < MenuItem Header = "Exit" Click = "FileExit_Click" />



            </ MenuItem >

            < MenuItem Header = "Help" >
                < MenuItem Header = "User Guide" Click = "HelpGuide_Click" />
            </ MenuItem >

            < MenuItem Header = "About" Click = "About_Click" />
        </ Menu >

        < !--Main Grid-- >
        < Grid DockPanel.Dock = "Bottom" >
            < Grid.ColumnDefinitions >
                < ColumnDefinition Width = "200" />
                < ColumnDefinition Width = "9*" />
                < ColumnDefinition Width = "791*" />
            </ Grid.ColumnDefinitions >

            < !--Navigation-- >
            < StackPanel Background = "#FF2D2D30" Grid.Column = "0" >
                < TextBlock Text = "GMS" Foreground = "White" FontSize = "22" Margin = "10" HorizontalAlignment = "Center" />
                < Button Content = "Files" Click = "FilesButton_Click" Margin = "10" />
                < Button Content = "Job" Click = "MainContent_Click" Margin = "10" />
                < Button Content = "HRMS" Click = "HContent_Click" Margin = "10" />
                < Button Content = "Accounts" Click = "AContent_Click" Margin = "10" />


            </ StackPanel >

            < !--Main Content Area -->
            <ContentControl x:Name = "MainContent" Grid.Column = "1" Grid.ColumnSpan = "2" />
        </ Grid >
    </ DockPanel >
</ Window >*/