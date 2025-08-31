<UserControl x:Class="GMSApp.Views.Job.JobOrder"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d"
             d:DesignHeight="700" d:DesignWidth="1000">

    <UserControl.Resources>
        <!-- Ensure you have a ByteArrayToImageConverter implemented in GMSApp.Converters namespace.
             If not, remove converter references or implement a converter that converts byte[] to ImageSource. -->
        <ResourceDictionary>
            <conv:ByteArrayToImageConverter x:Key="ByteArrayToImageConverter" xmlns:conv="clr-namespace:GMSApp.Converters"/>
        </ResourceDictionary>
    </UserControl.Resources>

    <Grid Margin="8">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Top toolbar -->
        <StackPanel Orientation="Horizontal" Grid.Row="0" Margin="0,0,0,8">
            <Button Content="Add Job" Width="110" Margin="0,0,8,0" Command="{Binding AddJoborderCommand}" />
            <Button Content="Save" Width="90" Margin="0,0,8,0" Command="{Binding SaveCommand}" />
            <Button Content="Update" Width="90" Margin="0,0,8,0" Command="{Binding UpdateJoborderCommand}" />
            <Button Content="Delete" Width="90" Margin="0,0,8,0" Command="{Binding DeleteJoborderCommand}" />
            <Button Content="Print" Width="90" Margin="8,0,0,0" Command="{Binding PrintCommand}" />
        </StackPanel>

        <!-- Main area -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="2*"/>
                <ColumnDefinition Width="3*"/>
            </Grid.ColumnDefinitions>

            <!-- Left: Job list -->
            <Border Grid.Column="0" BorderBrush="LightGray" BorderThickness="1" Padding="6" Margin="0,0,8,0">
                <DataGrid ItemsSource="{Binding Joborders}"
                          SelectedItem="{Binding SelectedJoborder, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                          AutoGenerateColumns="False"
                          CanUserAddRows="False"
                          CanUserDeleteRows="False"
                          IsReadOnly="True"
                          SelectionMode="Single">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Id" Binding="{Binding Id}" Width="60"/>
                        <DataGridTextColumn Header="Customer" Binding="{Binding CustomerName}" Width="*"/>
                        <DataGridTextColumn Header="Phone" Binding="{Binding Phonenumber}" Width="120"/>
                        <DataGridTextColumn Header="Vehicle" Binding="{Binding VehicleNumber}" Width="120"/>
                    </DataGrid.Columns>
                </DataGrid>
            </Border>

            <!-- Right: Details, items and images -->
            <StackPanel Grid.Column="1" Orientation="Vertical" Margin="0,0,0,0">
                <!-- Items section -->
                <GroupBox Header="Items" Margin="0,0,0,8">
                    <DockPanel>
                        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="6">
                            <Button Content="Add Item" Command="{Binding AddItemCommand}" Width="100"/>
                            <TextBlock Text=" " Width="8"/>
                            <Button Content="Save Items" Command="{Binding SaveCommand}" Width="100"/>
                            <TextBlock Text=" " Width="16"/>
                            <TextBlock Text="Total:" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding Total, StringFormat=N2}" FontWeight="Bold" Margin="6,0,0,0" VerticalAlignment="Center"/>
                        </StackPanel>

                        <DataGrid ItemsSource="{Binding Items}"
                                  AutoGenerateColumns="False"
                                  CanUserAddRows="False"
                                  Margin="6"
                                  Height="220">
                            <DataGrid.Columns>
                                <DataGridTextColumn Header="Name" Binding="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*" />
                                <DataGridTextColumn Header="Quantity" Binding="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="100" />
                                <DataGridTextColumn Header="Price" Binding="{Binding Price, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, StringFormat=N2}" Width="100" />
                                <DataGridTextColumn Header="Total" Binding="{Binding Total, StringFormat=N2}" IsReadOnly="True" Width="100" />
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
                    </DockPanel>
                </GroupBox>

                <!-- Details form -->
                <Border BorderBrush="LightGray" BorderThickness="1" Padding="10" Background="#FFF9F9F9" Margin="0,0,0,8">
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
                    </Grid>
                </Border>

                <!-- Images -->
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" Margin="0,0,0,0">
                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="120" Height="80"
                               Source="{Binding SelectedJoborder.F, Converter={StaticResource ByteArrayToImageConverter}}"
                               Stretch="Uniform" />
                        <Button Content="Front" Width="120" Margin="0,6,0,0" Command="{Binding FrontFileCommand}"/>
                    </StackPanel>

                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="120" Height="80"
                               Source="{Binding SelectedJoborder.B, Converter={StaticResource ByteArrayToImageConverter}}"
                               Stretch="Uniform" />
                        <Button Content="Back" Width="120" Margin="0,6,0,0" Command="{Binding BackFileCommand}"/>
                    </StackPanel>

                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="120" Height="80"
                               Source="{Binding SelectedJoborder.LS, Converter={StaticResource ByteArrayToImageConverter}}"
                               Stretch="Uniform" />
                        <Button Content="Left" Width="120" Margin="0,6,0,0" Command="{Binding LeftFileCommand}"/>
                    </StackPanel>

                    <StackPanel Orientation="Vertical" Margin="0,0,12,0">
                        <Image Width="120" Height="80"
                               Source="{Binding SelectedJoborder.RS, Converter={StaticResource ByteArrayToImageConverter}}"
                               Stretch="Uniform" />
                        <Button Content="Right" Width="120" Margin="0,6,0,0" Command="{Binding RightFileCommand}"/>
                    </StackPanel>
                </StackPanel>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>