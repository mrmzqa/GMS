
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMS25.Models;
using GMS25.Services;
using System.Collections.ObjectModel;

namespace GMS25.ViewModels
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly IPosService _posService;

        [ObservableProperty]
        private ObservableCollection<Order> _orders;

        [ObservableProperty]
        private Order _selectedOrder;

        public OrdersViewModel(IPosService posService)
        {
            _posService = posService;
            _ = LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            var orders = await _posService.GetOrdersAsync();
            Orders = new ObservableCollection<Order>(orders);
        }

        [RelayCommand]
        private async Task UpdateOrderStatus(string status)
        {
            if (SelectedOrder != null)
            {
                await _posService.UpdateOrderStatusAsync(SelectedOrder.OrderId, status);
                SelectedOrder.Status = status;
            }
        }
    }
}