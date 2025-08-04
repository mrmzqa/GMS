using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels
{
    public partial class ProductViewModel : ObservableObject
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private string? newProductName;

        [ObservableProperty]
        private decimal newProductPrice;

        [ObservableProperty]
        private Category? selectedCategoryForNewProduct;

        public ProductViewModel(IRepository<Product> productRepository,
                                IRepository<Category> categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _ = LoadAllAsync();
        }

        [RelayCommand]
        public async Task LoadAllAsync()
        {
            Products.Clear();
            Categories.Clear();

            var prods = await _productRepository.GetAllAsync();
            foreach (var p in prods)
                Products.Add(p);

            var cats = await _categoryRepository.GetAllAsync();
            foreach (var c in cats)
                Categories.Add(c);

            // Clear selections
            SelectedProduct = null;
            SelectedCategoryForNewProduct = null;
            NewProductName = string.Empty;
            NewProductPrice = 0;
        }

        [RelayCommand(CanExecute = nameof(CanAddProduct))]
        public async Task AddProductAsync()
        {
            if (!CanAddProduct()) return;
            var newProduct = new Product
            {
                Name = NewProductName!.Trim(),
                Price = NewProductPrice,
                CategoryId = SelectedCategoryForNewProduct!.Id
            };

            await _productRepository.AddAsync(newProduct);
            await LoadAllAsync();
        }

        private bool CanAddProduct()
        {
            return !string.IsNullOrWhiteSpace(NewProductName) && NewProductPrice >= 0 && SelectedCategoryForNewProduct != null;
        }

        [RelayCommand(CanExecute = nameof(CanEditDelete))]
        public async Task UpdateProductAsync()
        {
            if (SelectedProduct == null) return;
            if (string.IsNullOrWhiteSpace(SelectedProduct.Name))
            {
                MessageBox.Show("Product name cannot be empty.");
                return;
            }

            await _productRepository.UpdateAsync(SelectedProduct);
            await LoadAllAsync();
        }

        [RelayCommand(CanExecute = nameof(CanEditDelete))]
        public async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;
            await _productRepository.DeleteAsync(SelectedProduct.Id);
            SelectedProduct = null;
            await LoadAllAsync();
        }

        private bool CanEditDelete() => SelectedProduct != null;
    }
}