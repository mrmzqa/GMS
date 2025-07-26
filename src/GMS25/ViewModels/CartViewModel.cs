using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMS25.Models;
using GMS25.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.ViewModels
{
    public partial class CartViewModel : ObservableObject
    {
        private readonly IPosService _posService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private ObservableCollection<CartItem> _cartItems = new();

        [ObservableProperty]
        private decimal _total;

        public CartViewModel(IPosService posService, IAuthService authService)
        {
            _posService = posService;
            _authService = authService;
        }

        public void AddToCart(Product product, int quantity = 1)
        {
            var existingItem = CartItems.FirstOrDefault(i => i.Product.ProductId == product.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                CartItems.Add(new CartItem { Product = product, Quantity = quantity });
            }
            CalculateTotal();
        }

        public void RemoveFromCart(CartItem item)
        {
            CartItems.Remove(item);
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            Total = CartItems.Sum(i => i.Product.Price * i.Quantity);
        }

        [RelayCommand]
        private async Task Checkout()
        {
            if (CartItems.Count == 0) return;

            var order = new Order
            {
                UserId = _authService.CurrentUser?.UserId,
                TotalAmount = Total
            };

            var orderItems = CartItems.Select(i => new OrderItem
            {
                ProductId = i.Product.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Product.Price
            }).ToList();

            await _posService.CreateOrderAsync(order, orderItems);
            CartItems.Clear();
            Total = 0;
        }
    }

    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}