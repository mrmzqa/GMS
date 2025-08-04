using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Models;
using GMSApp.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GMSApp.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private readonly IRepository<Category> _categoryRepository;

        public ObservableCollection<Category> Categories { get; } = new();

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private string? newCategoryName;

        public CategoryViewModel(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
            _ = LoadAllAsync();
        }

        [RelayCommand]
        public async Task LoadAllAsync()
        {
            Categories.Clear();
            var items = await _categoryRepository.GetAllAsync();
            foreach (var item in items)
                Categories.Add(item);
        }

        [RelayCommand(CanExecute = nameof(CanAddCategory))]
        public async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
            var category = new Category { Name = NewCategoryName.Trim() };
            await _categoryRepository.AddAsync(category);
            NewCategoryName = string.Empty;
            await LoadAllAsync();
        }

        private bool CanAddCategory() => !string.IsNullOrWhiteSpace(NewCategoryName);

        [RelayCommand(CanExecute = nameof(CanEditDelete))]
        public async Task UpdateCategoryAsync()
        {
            if (SelectedCategory == null) return;
            if (string.IsNullOrWhiteSpace(SelectedCategory.Name))
            {
                MessageBox.Show("Category name cannot be empty.");
                return;
            }
            await _categoryRepository.UpdateAsync(SelectedCategory);
            await LoadAllAsync();
        }
        
        [RelayCommand(CanExecute = nameof(CanEditDelete))]
        public async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;
            await _categoryRepository.DeleteAsync(SelectedCategory.Id);
            SelectedCategory = null;
            await LoadAllAsync();
        }

        private bool CanEditDelete() => SelectedCategory != null;
    }
}