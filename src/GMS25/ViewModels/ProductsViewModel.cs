using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.ViewModels
{
    internal class ProductsViewModel
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
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly IPosService _posService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private Product _selectedProduct;

        public ProductsViewModel(IPosService posService)
        {
            _posService = posService;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var products = await _posService.GetProductsAsync();
            Products = new ObservableCollection<Product>(products);

            var categories = await _posService.GetCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);
        }

        [RelayCommand]
        private async Task AddProduct()
        {
            var newProduct = new Product { Name = "New Product", Price = 0, StockQuantity = 0 };
            await _posService.AddProductAsync(newProduct);
            Products.Add(newProduct);
            SelectedProduct = newProduct;
        }

        [RelayCommand]
        private async Task SaveProduct()
        {
            if (SelectedProduct != null)
            {
                await _posService.UpdateProductAsync(SelectedProduct);
            }
        }

        [RelayCommand]
        private async Task DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                await _posService.DeleteProductAsync(SelectedProduct.ProductId);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
            }
        }
    }
}