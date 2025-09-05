<UserControl x:Class="GMSApp.Views.Inventory.InventoryItemView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             d:DesignHeight="600" d:DesignWidth="1000">
    <Grid Margin="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="360"/>
            <ColumnDefinition Width="12"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Left: list and actions -->
        <Border Grid.Column="0" Padding="8" BorderBrush="#DDD" Background="WhiteSmoke">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <Button Content="Reload" Command="{Binding LoadCommand}" Width="80" Margin="0,0,6,0"/>
                    <Button Content="New" Command="{Binding AddCommand}" Width="80" Margin="0,0,6,0"/>
                    <Button Content="Save" Command="{Binding SaveCommand}" Width="80" Margin="0,0,6,0"/>
                    <Button Content="Delete" Command="{Binding DeleteCommand}" Width="80" />
                </StackPanel>

                <DataGrid ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                          AutoGenerateColumns="False" CanUserAddRows="False" MinHeight="400">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="Code" Binding="{Binding ItemCode}" Width="120"/>
                        <DataGridTextColumn Header="Name" Binding="{Binding Name}" Width="*" />
                        <DataGridTextColumn Header="Qty" Binding="{Binding QuantityInStock}" Width="80"/>
                    </DataGrid.Columns>
                </DataGrid>
            </StackPanel>
        </Border>

        <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Center" VerticalAlignment="Stretch"/>

        <!-- Right: details & adjustment -->
        <Border Grid.Column="2" Padding="12" BorderBrush="#DDD">
            <StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Code:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.ItemCode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="240"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Name:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="420"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Category:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.Category, Mode=TwoWay}" Width="180"/>
                    <TextBlock Text="SubCategory:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.SubCategory, Mode=TwoWay}" Width="180"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Qty In Stock:" Width="120" VerticalAlignment="Center"/>
                    <TextBlock Text="{Binding SelectedItem.QuantityInStock}" Width="80" />
                    <TextBlock Text="Reorder Level:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.ReorderLevel, Mode=TwoWay}" Width="80"/>
                    <TextBlock Text="Unit:" Width="60" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.Unit, Mode=TwoWay}" Width="80"/>
                </StackPanel>

                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="Cost Price:" Width="120" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.CostPrice, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                    <TextBlock Text="Selling Price:" Width="120" Margin="12,0,0,0" VerticalAlignment="Center"/>
                    <TextBox Text="{Binding SelectedItem.SellingPrice, Mode=TwoWay, StringFormat=N2}" Width="120"/>
                </StackPanel>

                <GroupBox Header="Manual Stock Adjustment" Margin="0,12,0,0">
                    <StackPanel Orientation="Horizontal" Margin="8">
                        <TextBlock Text="Quantity (±):" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding AdjustmentQuantity, Mode=TwoWay}" Width="80" Margin="6,0,10,0"/>
                        <TextBlock Text="Notes:" VerticalAlignment="Center"/>
                        <TextBox Text="{Binding AdjustmentNotes, Mode=TwoWay}" Width="360" Margin="6,0,10,0"/>
                        <Button Content="Apply" Command="{Binding AdjustStockCommand}" Width="100"/>
                    </StackPanel>
                    <TextBlock Text="Positive quantity adds stock, negative removes." FontStyle="Italic" Margin="8,4"/>
                </GroupBox>

                <TextBlock Text="{Binding NeedsReorder, Converter