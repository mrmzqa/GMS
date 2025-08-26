using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;
namespace GMSApp.Repositories;

public class GenericPdfGenerator<T> : IGenericPdfGenerator<T> where T : class
{
    public async Task GeneratePdfAsync(IEnumerable<T> items, string filePath)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text($"{typeof(T).Name} List").FontSize(18).Bold().Underline();

                        foreach (var item in items)
                        {
                            column.Item().Border(1).Padding(5).Column(row =>
                            {
                                foreach (var prop in properties)
                                {
                                    var value = prop.GetValue(item)?.ToString() ?? "null";
                                    row.Item().Text($"{prop.Name}: {value}");
                                }
                            });
                        }
                    });
            });
        }).GeneratePdf(filePath);
    }
}
