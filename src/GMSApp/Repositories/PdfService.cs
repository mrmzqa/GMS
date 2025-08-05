using GMSApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace GMSApp.Repositories;
public class PdfService
{
    public static byte[] GeneratePurchaseOrderPdf(PurchaseOrder order)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().Text($"Purchase Order #{order.OrderNumber}").FontSize(20).Bold();
                        col.Item().Text($"Date: {order.Date:d}");
                        col.Item().LineHorizontal(1);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); // Name
                                c.RelativeColumn(1); // Qty
                                c.RelativeColumn(2); // Price
                                c.RelativeColumn(2); // Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Item");
                                header.Cell().Text("Qty");
                                header.Cell().Text("Price");
                                header.Cell().Text("Total");
                            });

                            foreach (var item in order.Items)
                            {
                                table.Cell().Text(item.Name);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text(item.Price.ToString("F2"));
                                table.Cell().Text(item.Total.ToString("F2"));
                            }
                        });

                        col.Item().PaddingTop(10).AlignRight().Text($"Grand Total: {order.GrandTotal:C}").FontSize(14).Bold();
                    });
            });
        });

        return doc.GeneratePdf();
    }
}