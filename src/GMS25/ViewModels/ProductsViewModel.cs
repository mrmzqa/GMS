using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMS25.Models;
using GMS25.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GMS25.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly IPosService _posService;

        // Manually defining the backing fields
        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private bool _isLoading;
        private Product _selectedProduct;

        // Public properties for the ViewModel

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Constructor that initializes the ViewModel and starts loading data asynchronously
        public ProductsViewModel(IPosService posService)
        {
            _posService = posService;
            _ = LoadDataAsync(); // Initiate async data loading
        }

        // Asynchronous method to load products and categories
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true; // Set loading state to true

                // Fetch products and categories from the service asynchronously
                var products = await _posService.GetProductsAsync();
                Products = new ObservableCollection<Product>(products);

                var categories = await _posService.GetCategoriesAsync();
                Categories = new ObservableCollection<Category>(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Set loading state to false after data load is complete
            }
        }

        // Command to add a new product
        [RelayCommand]
        private async Task AddProduct()
        {
            try
            {
                var newProduct = new Product { Name = "New Product", Price = 0, StockQuantity = 0 };

                // Add new product using the service
                await _posService.AddProductAsync(newProduct);
                Products.Add(newProduct);
                SelectedProduct = newProduct;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Command to save changes to an existing product
        [RelayCommand]
        private async Task SaveProduct()
        {
            if (SelectedProduct != null)
            {
                try
                {
                    await _posService.UpdateProductAsync(SelectedProduct);
                    MessageBox.Show("Product updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Command to delete a selected product
        [RelayCommand]
        private async Task DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                try
                {
                    await _posService.DeleteProductAsync(SelectedProduct.ProductId);
                    Products.Remove(SelectedProduct);
                    SelectedProduct = null;
                    MessageBox.Show("Product deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
