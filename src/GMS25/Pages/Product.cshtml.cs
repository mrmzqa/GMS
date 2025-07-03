using GMS.Models.Models;
using GMS.Repositories.Repositories;
using GMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GMS25.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IFileService _fileService;

        public ProductsModel(IRepository<Product> productRepository, IFileService fileService)
        {
            _productRepository = productRepository;
            _fileService = fileService;
        }

        [BindProperty]
        public Product Product { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public async Task OnGetAsync()
        {
            Products = await _productRepository.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (ImageFile != null)
            {
                Product.ImagePath = await _fileService.SaveFileAsync(ImageFile, "uploads/products");
            }

            await _productRepository.AddAsync(Product);
            await _productRepository.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    await _fileService.DeleteFileAsync(product.ImagePath);
                }

                await _productRepository.RemoveAsync(id);
                await _productRepository.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
