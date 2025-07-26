using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.ViewModels
{
    internal class OrdersViewModel
    {
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WpfPosApp.Models;
using WpfPosApp.Services;

namespace WpfPosApp.ViewModels
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