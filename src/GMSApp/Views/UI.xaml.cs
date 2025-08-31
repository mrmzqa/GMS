<UserControl x:Class="GMSApp.Views.Job.JobOrder"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:GMSApp.ViewModels.Job"
             xmlns:conv="clr-namespace:GMSApp.Converters"
             mc:Ignorable="d"
             d:DesignHeight="700" d:DesignWidth="1000">

    <UserControl.Resources>
        <conv:ByteArrayToImageConverter x:Key="ByteArrayToImageConverter" />
    </UserControl.Resources>

    <Grid Margin="8">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Top toolbar -->
        <StackPanel Orientation="Horizontal" Grid.Row="0" Margin="0,0,0,8">
            <Label Content="Search:" VerticalAlignment="Center" />
            <TextBox Width="220" Margin="6,0,0,0" x:Name="SearchBox"/>
            <Button Content="Clear" Margin="6,0,0,0" Width="60"/>
            <Button Content="Add Job" Margin="16,0,0,0" Width="110" Command="{Binding AddJoborderCommand}" />
            <Button Content="Print" Margin="8,0,0,0" Width="80" Command="{Binding PrintCommand}" />
        </StackPanel>

        <!-- Main area: left = job list, right = details & items -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="2*"/>
                <ColumnDefinition Width="3*"/>
            </Grid.ColumnDefinitions>

            <!-- Job list -->
            <DataGrid Grid.Column="0"
                      ItemsSource="{Binding Joborders}"
                      SelectedItem="{Binding SelectedJoborder, Mode=TwoWay}"
                      AutoGenerateColumns="True"
                      CanUserAddRows="False"
                      CanUserDeleteRows="False"
                      IsReadOnly="True"
                      SelectionMode="Single"
                      Margin="0,0,8,0" />

            <!-- Detail & items -->
            <StackPanel Grid.Column="1" Orientation="Vertical">
                <!-- Items grid -->
                <DataGrid ItemsSource="{Binding Items}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          Height="220"
                          Margin="0,0,0,8">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                        <DataGridTextColumn Header="Quantity" Binding="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="100"/>
                        <DataGridTextColumn Header="Price" Binding="{Binding Price, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="100"/>
                        <DataGridTextColumn Header="Total" Binding="{Binding Total, StringFormat=N2}" IsReadOnly="True" Width="100"/>
                        <DataGridTemplateColumn Header="Action" Width="90">
                            <DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <Button Content="Delete"
                                            Command="{Binding DataContext.RemoveItemCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                            CommandParameter="{Binding}"
                                            Width="70" />
                                </DataTemplate>
                            </DataGridTemplateColumn.CellTemplate>
                        </DataGridTemplateColumn>
                    </DataGrid.Columns>
                </DataGrid>

                <!-- Items actions and total -->
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <Button Content="Add Item" Width="100" Command="{Binding AddItemCommand}" />
                    <Button Content="Save Items" Width="110" Margin="8,0,0,0" Command="{Binding SaveCommand}" />
                    <TextBlock Text="Total:" VerticalAlignment="Center" Margin="24,0,0,0"/>
                    <TextBlock Text="{Binding Total, StringFormat=N2}" VerticalAlignment="Center" Margin="6,0,0,0" FontWeight="Bold"/>
                </StackPanel>

                <!-- Details form -->
                <Border BorderBrush="LightGray" BorderThickness="1" Padding="10" Background="#FFF9F9F9">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="140"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="140"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <Label Content="Customer:" Grid.Row="0" Grid.Column="0" VerticalAlignment="Center"/>
                        <TextBox Grid.Row="0" Grid.Column="1" Margin="6,4" Text="{Binding SelectedJoborder.CustomerName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

                        <Label Content="Phone:" Grid.Row="0" Grid.Column="2" VerticalAlignment="Center"/>
                        <TextBox Grid.Row="0" Grid.Column="3" Margin="6,4" Text="{Binding SelectedJoborder.Phonenumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

                        <Label Content="Vehicle No:" Grid.Row="1" Grid.Column="0" VerticalAlignment="Center"/>
                        <TextBox Grid.Row="1" Grid.Column="1" Margin="6,4" Text="{Binding SelectedJoborder.VehicleNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

                        <Label Content="Brand:" Grid.Row="1" Grid.Column="2" VerticalAlignment="Center"/>
                        <TextBox Grid.Row="1" Grid.Column="3" Margin="6,4" Text="{Binding SelectedJoborder.Brand, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

                        <Label Content="Model:" Grid.Row="2" Grid.Column="0" VerticalAlignment="Center"/>
                        <TextBox Grid.Row="2" Grid.Column="1" Margin="6,4" Text="{Binding SelectedJoborder.Model, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

                        <Label Content="Odometer:" Grid.Row="2" Grid.Column="2" VerticalAlignment="Center"/>
                        <TextBox Grid.Row="2" Grid.Column="3" Margin="6,4" Text="{Binding SelectedJoborder.OdoNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

                        <StackPanel Orientation="Horizontal" Grid.Row="3" Grid.Column="3" HorizontalAlignment="Right" Margin="0,8,0,0">
                            <Button Content="Update" Width="100" Margin="0,0,8,0" Command="{Binding UpdateJoborderCommand}" />
                            <Button Content="Delete" Width="100" Command="{Binding DeleteJoborderCommand}" />
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Images and file buttons -->
                <StackPanel Orientation="Horizontal" Margin="0,8,0,0" VerticalAlignment="Top">
                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="96" Height="64" Source="{Binding SelectedJoborder.F, Converter={StaticResource ByteArrayToImageConverter}}" Stretch="Uniform"/>
                        <Button Content="F" Width="96" Margin="0,4,0,0" Command="{Binding FrontFileCommand}" />
                    </StackPanel>

                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="96" Height="64" Source="{Binding SelectedJoborder.B, Converter={StaticResource ByteArrayToImageConverter}}" Stretch="Uniform"/>
                        <Button Content="B" Width="96" Margin="0,4,0,0" Command="{Binding BackFileCommand}" />
                    </StackPanel>

                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="96" Height="64" Source="{Binding SelectedJoborder.LS, Converter={StaticResource ByteArrayToImageConverter}}" Stretch="Uniform"/>
                        <Button Content="L" Width="96" Margin="0,4,0,0" Command="{Binding LeftFileCommand}" />
                    </StackPanel>

                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="96" Height="64" Source="{Binding SelectedJoborder.RS, Converter={StaticResource ByteArrayToImageConverter}}" Stretch="Uniform"/>
                        <Button Content="R" Width="96" Margin="0,4,0,0" Command="{Binding RightFileCommand}" />
                    </StackPanel>
                </StackPanel>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
using GMSApp.ViewModels.Job;
using System.Windows.Controls;

namespace GMSApp.Views.Job
{
    public partial class JobOrder : UserControl
    {
        public JobOrder()
        {
            InitializeComponent();
        }

        // Use this ctor when resolving the VM via DI
        public JobOrder(JoborderViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
