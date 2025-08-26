
namespace GMSApp.Repositories;

public interface IGenericPdfGenerator<T> where T : class
{
    Task GeneratePdfAsync(IEnumerable<T> items, string filePath);
}
