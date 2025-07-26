using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMS25.Models;

namespace GMS25.Services
{
    public interface IPosService
    {
        Task<List<Product>> GetProductsAsync();
        Task<List<Category>> GetCategoriesAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        
        Task CreateOrderAsync(Order order, List<OrderItem> items);
        Task<List<Order>> GetOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task UpdateOrderStatusAsync(int orderId, string status);
    }
}
}
