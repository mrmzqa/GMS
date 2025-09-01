// If you still want the generic registration for other types keep it, but ensure Joborder uses the concrete generator:
services.AddTransient<IGenericPdfGenerator<GMSApp.Models.job.Joborder>, GMSApp.Repositories.Pdf.JoborderPdfGenerator>();
// (optional) keep other open-generic registrations if you have other generators
services.AddTransient(typeof(IGenericPdfGenerator<>), typeof(GenericPdfGenerator<>));


[RelayCommand(CanExecute = nameof(CanModify))]
public async Task PrintAsync()
{
    if (SelectedJoborder == null)
        return;

    try
    {
        // Build a fresh Joborder model containing the current Items (UI edits)
        var model = BuildJoborderFromUi(SelectedJoborder); // your existing helper that returns Joborder with Items

        // Build file path
        var temp = Path.Combine(Path.GetTempPath(), $"joborder_{model.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        // Use the injected PDF generator (ensure DI registers JoborderPdfGenerator for Joborder)
        await _pdfGenerator.GeneratePdfAsync(new[] { model }, temp);

        // Open the generated PDF using the default system app
        var psi = new ProcessStartInfo(temp) { UseShellExecute = true };
        Process.Start(psi);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Failed to generate/print PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}


using GMSApp.Models.job;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GMSApp.Repositories.Pdf
{
    // Assumed interface:
    // public interface IGenericPdfGenerator<T> { Task GeneratePdfAsync(IEnumerable<T> models, string filePath); }
    public class JoborderPdfGenerator : IGenericPdfGenerator<Joborder>
    {
        public async Task GeneratePdfAsync(IEnumerable<Joborder> models, string filePath)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            await Task.Run(() =>
            {
                using var document = new PdfDocument();

                foreach (var job in models)
                {
                    // Start a new page for each job
                    var page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    page.Orientation = PdfSharpCore.PageOrientation.Portrait;

                    using var gfx = XGraphics.FromPdfPage(page);

                    // Layout settings
                    double marginLeft = 40;
                    double marginTop = 40;
                    double marginRight = 40;
                    double marginBottom = 40;
                    double pageWidth = page.Width;
                    double pageHeight = page.Height;
                    double usableWidth = pageWidth - marginLeft - marginRight;
                    double y = marginTop;

                    // Fonts
                    var headerFont = new XFont("Arial", 16, XFontStyle.Bold);
                    var subHeaderFont = new XFont("Arial", 11, XFontStyle.Bold);
                    var normalFont = new XFont("Arial", 10, XFontStyle.Regular);
                    var smallFont = new XFont("Arial", 9, XFontStyle.Regular);

                    // Header: Company / Title centered
                    var title = "JOB CARD";
                    gfx.DrawString(title, headerFont, XBrushes.Black,
                        new XRect(marginLeft, y, usableWidth, 24), XStringFormats.TopCenter);
                    y += 28;

                    // Draw a thin separator
                    gfx.DrawLine(XPens.Black, marginLeft, y, marginLeft + usableWidth, y);
                    y += 8;

                    // Optional image on top-right if available (front image)
                    double imgWidth = 120;
                    double imgHeight = 80;
                    if (job.F != null && job.F.Length > 0)
                    {
                        try
                        {
                            using var ms = new MemoryStream(job.F);
                            using var img = XImage.FromStream(() => ms);
                            double imgX = marginLeft + usableWidth - imgWidth;
                            gfx.DrawImage(img, imgX, marginTop + 4, imgWidth, imgHeight);
                        }
                        catch
                        {
                            // ignore image load errors; continue without image
                        }
                    }

                    // Job Details block - left side
                    double detailsX = marginLeft;
                    double detailsWidth = usableWidth - imgWidth - 12; // leave space for image
                    double lineHeight = 16;

                    void DrawLabelValue(string label, string? value)
                    {
                        gfx.DrawString(label + ":", subHeaderFont, XBrushes.Black, new XPoint(detailsX, y));
                        gfx.DrawString(value ?? string.Empty, normalFont, XBrushes.Black, new XPoint(detailsX + 100, y));
                        y += lineHeight;
                    }

                    DrawLabelValue("Customer", job.CustomerName);
                    DrawLabelValue("Phone", job.Phonenumber);
                    DrawLabelValue("Vehicle No", job.VehicleNumber);
                    DrawLabelValue("Brand", job.Brand);
                    DrawLabelValue("Model", job.Model);
                    DrawLabelValue("Odometer", job.OdoNumber?.ToString() ?? string.Empty);

                    y += 6;

                    // Another separator before items
                    gfx.DrawLine(XPens.Gray, marginLeft, y, marginLeft + usableWidth, y);
                    y += 8;

                    // Items table header
                    double tableX = marginLeft;
                    double colNameW = usableWidth * 0.55;   // Name column
                    double colQtyW = usableWidth * 0.12;    // Qty
                    double colPriceW = usableWidth * 0.16;  // Price
                    double colTotalW = usableWidth * 0.17;  // Total
                    double rowHeight = 20;

                    // Header background
                    var headerRect = new XRect(tableX, y, usableWidth, rowHeight);
                    gfx.DrawRectangle(XBrushes.LightGray, headerRect);

                    // Draw column headers
                    double cx = tableX;
                    gfx.DrawString("Item", subHeaderFont, XBrushes.Black, new XRect(cx + 4, y + 4, colNameW, rowHeight), XStringFormats.TopLeft);
                    cx += colNameW;
                    gfx.DrawString("Qty", subHeaderFont, XBrushes.Black, new XRect(cx + 4, y + 4, colQtyW, rowHeight), XStringFormats.TopLeft);
                    cx += colQtyW;
                    gfx.DrawString("Price", subHeaderFont, XBrushes.Black, new XRect(cx + 4, y + 4, colPriceW, rowHeight), XStringFormats.TopLeft);
                    cx += colPriceW;
                    gfx.DrawString("Total", subHeaderFont, XBrushes.Black, new XRect(cx + 4, y + 4, colTotalW, rowHeight), XStringFormats.TopLeft);

                    y += rowHeight;

                    // Items rows: paginate if necessary
                    var items = job.Items?.ToList() ?? new List<GMSApp.Models.ItemRow>();
                    decimal grandTotal = 0m;
                    foreach (var it in items)
                    {
                        // Check for page overflow: leave marginBottom + footer space (~40)
                        if (y + rowHeight + marginBottom + 40 > pageHeight)
                        {
                            // Draw footer for this page, then create a new page and headers
                            DrawFooter(gfx, pageWidth, pageHeight, marginLeft, marginRight, smallFont, 1, document.PageCount);
                            // Add new page
                            var newPage = document.AddPage();
                            newPage.Size = PdfSharpCore.PageSize.A4;
                            using var newGfx = XGraphics.FromPdfPage(newPage);
                            // Replace gfx with new page graphics
                            gfx.Dispose();
                            // Note: reuse the same gfx variable by creating a new one referencing new page
                            // But since gfx is declared in outer scope, we need to update it. Simplest is to
                            // reassign via reflection-like behaviour; here we'll just set to newGfx by shadowing variable:
                            // Instead, break out and continue with fresh page variables. Simpler approach: draw footer and then
                            // manually continue on the new page by setting gfx = newGfx, page = newPage, y = marginTop + header height.
                            // But because gfx is within using, we'll rewrite this logic a bit simpler below.
                        }

                        // Since the above complex pagination inside loop is tricky with using and disposals,
                        // we will implement a simpler, robust pagination approach below: collect all lines into a queue
                        // and render them while tracking y; when page full, create new page and continue.
                    }

                    // Simpler rendering: render rows with safe pagination using a helper function
                    RenderItemsTableWithPagination(document, ref page, ref gfx, ref y, marginLeft, marginTop, marginRight, marginBottom,
                        usableWidth, items, colNameW, colQtyW, colPriceW, colTotalW, rowHeight, normalFont, smallFont, subHeaderFont, ref grandTotal);

                    // After items rendered, draw grand total
                    y += 8;
                    gfx.DrawLine(XPens.Gray, marginLeft, y, marginLeft + usableWidth, y);
                    y += 6;
                    gfx.DrawString("Grand Total:", subHeaderFont, XBrushes.Black, new XPoint(marginLeft + usableWidth - 200, y));
                    gfx.DrawString(grandTotal.ToString("N2"), subHeaderFont, XBrushes.Black, new XPoint(marginLeft + usableWidth - 80, y));

                    // Footer with page number
                    DrawFooter(gfx, pageWidth, pageHeight, marginLeft, marginRight, smallFont, 1, document.PageCount);
                }

                // Save document
                // Ensure directory exists
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var stream = File.Create(filePath);
                document.Save(stream);
            });
        }

        // Helper: render items with pagination (creates new pages as needed)
        private void RenderItemsTableWithPagination(PdfDocument document,
                                                    ref PdfPage page,
                                                    ref XGraphics gfx,
                                                    ref double y,
                                                    double marginLeft,
                                                    double marginTop,
                                                    double marginRight,
                                                    double marginBottom,
                                                    double usableWidth,
                                                    List<GMSApp.Models.ItemRow> items,
                                                    double colNameW, double colQtyW, double colPriceW, double colTotalW,
                                                    double rowHeight,
                                                    XFont rowFont,
                                                    XFont smallFont,
                                                    XFont headerFont,
                                                    ref decimal grandTotal)
        {
            double pageWidth = page.Width;
            double pageHeight = page.Height;
            int pageNumber = document.PageCount;

            // Draw table header on current page (assuming header drawn above already)
            // If y is near top (i.e., after job details), we assume header already present. Otherwise draw header row.
            // For reliability, draw header row before the rows block.
            var headerRect = new XRect(marginLeft, y, usableWidth, rowHeight);
            gfx.DrawRectangle(XBrushes.LightGray, headerRect);
            double cx = marginLeft;
            gfx.DrawString("Item", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colNameW, rowHeight), XStringFormats.TopLeft);
            cx += colNameW;
            gfx.DrawString("Qty", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colQtyW, rowHeight), XStringFormats.TopLeft);
            cx += colQtyW;
            gfx.DrawString("Price", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colPriceW, rowHeight), XStringFormats.TopLeft);
            cx += colPriceW;
            gfx.DrawString("Total", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colTotalW, rowHeight), XStringFormats.TopLeft);
            y += rowHeight;

            foreach (var it in items)
            {
                // Check for overflow
                if (y + rowHeight + marginBottom + 40 > pageHeight)
                {
                    // Draw footer for current page number
                    DrawFooter(gfx, pageWidth, pageHeight, marginLeft, marginRight, smallFont, pageNumber, document.PageCount);
                    // Start new page
                    page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx.Dispose();
                    gfx = XGraphics.FromPdfPage(page);
                    pageNumber++;
                    // Reset y to top margin and draw header/title separator region
                    y = marginTop;
                    // Draw small title on new page to indicate continuation
                    gfx.DrawString("Continued - Items", new XFont("Arial", 12, XFontStyle.Bold), XBrushes.Black, new XRect(marginLeft, y, usableWidth, 20), XStringFormats.TopLeft);
                    y += 22;
                    // Draw table header on new page
                    headerRect = new XRect(marginLeft, y, usableWidth, rowHeight);
                    gfx.DrawRectangle(XBrushes.LightGray, headerRect);
                    cx = marginLeft;
                    gfx.DrawString("Item", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colNameW, rowHeight), XStringFormats.TopLeft);
                    cx += colNameW;
                    gfx.DrawString("Qty", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colQtyW, rowHeight), XStringFormats.TopLeft);
                    cx += colQtyW;
                    gfx.DrawString("Price", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colPriceW, rowHeight), XStringFormats.TopLeft);
                    cx += colPriceW;
                    gfx.DrawString("Total", headerFont, XBrushes.Black, new XRect(cx + 4, y + 4, colTotalW, rowHeight), XStringFormats.TopLeft);
                    y += rowHeight;
                }

                // Draw the row
                cx = marginLeft;
                gfx.DrawString(it.Name ?? string.Empty, rowFont, XBrushes.Black, new XRect(cx + 4, y + 4, colNameW, rowHeight), XStringFormats.TopLeft);
                cx += colNameW;
                gfx.DrawString(it.Quantity.ToString(), rowFont, XBrushes.Black, new XRect(cx + 4, y + 4, colQtyW, rowHeight), XStringFormats.TopLeft);
                cx += colQtyW;
                gfx.DrawString(it.Price.ToString("N2"), rowFont, XBrushes.Black, new XRect(cx + 4, y + 4, colPriceW, rowHeight), XStringFormats.TopLeft);
                cx += colPriceW;
                var total = (it.Quantity * it.Price);
                gfx.DrawString(total.ToString("N2"), rowFont, XBrushes.Black, new XRect(cx + 4, y + 4, colTotalW, rowHeight), XStringFormats.TopLeft);

                grandTotal += total;
                y += rowHeight;
            }

            // draw footer for final page (pageNumber is current)
            DrawFooter(gfx, pageWidth, pageHeight, marginLeft, marginRight, smallFont, pageNumber, document.PageCount);
        }

        private void DrawFooter(XGraphics gfx, double pageWidth, double pageHeight, double marginLeft, double marginRight, XFont smallFont, int pageNumber, int pageCount)
        {
            var footerY = pageHeight - 30;
            var footerText = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}    Page {pageNumber} of {pageCount}";
            gfx.DrawString(footerText, smallFont, XBrushes.Gray, new XRect(marginLeft, footerY, pageWidth - marginLeft - marginRight, 16), XStringFormats.Center);
        }
    }
}