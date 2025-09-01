using GMSApp.Models.job;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GMSApp.Repositories.Pdf
{
    // Ensure this matches your app's interface:
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
                    // Start first page for this job
                    PdfPage page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    try
                    {
                        double marginLeft = 40;
                        double marginTop = 40;
                        double marginRight = 40;
                        double marginBottom = 40;
                        double pageWidth = page.Width;
                        double pageHeight = page.Height;
                        double usableWidth = pageWidth - marginLeft - marginRight;
                        double y = marginTop;

                        // Fonts (use common system fonts)
                        var labelFont = new XFont("Arial", 10, XFontStyle.Bold);
                        var valueFont = new XFont("Arial", 10, XFontStyle.Regular);
                        var tableHeaderFont = new XFont("Arial", 10, XFontStyle.Bold);
                        var tableFont = new XFont("Arial", 10, XFontStyle.Regular);

                        // Draw job details (simple label:value rows, no header/footer)
                        void DrawRow(string label, string? value)
                        {
                            gfx.DrawString(label + ":", labelFont, XBrushes.Black, new XPoint(marginLeft, y));
                            gfx.DrawString(value ?? string.Empty, valueFont, XBrushes.Black, new XPoint(marginLeft + 120, y));
                            y += 18;
                        }

                        DrawRow("Customer", job.CustomerName);
                        DrawRow("Phone", job.Phonenumber);
                        DrawRow("Vehicle No", job.VehicleNumber);
                        DrawRow("Brand", job.Brand);
                        DrawRow("Model", job.Model);
                        DrawRow("Odometer", job.OdoNumber?.ToString());

                        // Add a small gap before items
                        y += 8;

                        // If front image available, draw it at top-right (keep simple)
                        double imageWidth = 120;
                        double imageHeight = 80;
                        if (job.F != null && job.F.Length > 0)
                        {
                            try
                            {
                                using var ms = new MemoryStream(job.F);
                                using var img = XImage.FromStream(() => ms);
                                double imgX = marginLeft + usableWidth - imageWidth;
                                double imgY = marginTop; // top aligned
                                gfx.DrawImage(img, imgX, imgY, imageWidth, imageHeight);

                                // Ensure we avoid overlapping image when y is small
                                if (y < imgY + imageHeight) y = imgY + imageHeight + 8;
                            }
                            catch
                            {
                                // ignore image errors
                            }
                        }

                        // Items table column widths
                        var items = job.Items?.ToList() ?? new List<GMSApp.Models.ItemRow>();
                        double colNameW = usableWidth * 0.55;
                        double colQtyW = usableWidth * 0.12;
                        double colPriceW = usableWidth * 0.16;
                        double colTotalW = usableWidth * 0.17;
                        double rowHeight = 20;

                        // Draw table header (simple)
                        void DrawTableHeader()
                        {
                            double cx = marginLeft;
                            gfx.DrawString("Item", tableHeaderFont, XBrushes.Black, new XRect(cx, y, colNameW, rowHeight), XStringFormats.TopLeft);
                            cx += colNameW;
                            gfx.DrawString("Qty", tableHeaderFont, XBrushes.Black, new XRect(cx, y, colQtyW, rowHeight), XStringFormats.TopLeft);
                            cx += colQtyW;
                            gfx.DrawString("Price", tableHeaderFont, XBrushes.Black, new XRect(cx, y, colPriceW, rowHeight), XStringFormats.TopLeft);
                            cx += colPriceW;
                            gfx.DrawString("Total", tableHeaderFont, XBrushes.Black, new XRect(cx, y, colTotalW, rowHeight), XStringFormats.TopLeft);
                            y += rowHeight;
                        }

                        DrawTableHeader();

                        decimal grandTotal = 0m;
                        int pageNumber = document.PageCount;

                        foreach (var it in items)
                        {
                            // Check for page overflow. Reserve some bottom space (marginBottom)
                            if (y + rowHeight + marginBottom > pageHeight)
                            {
                                // Dispose current gfx and create a new page
                                gfx.Dispose();
                                page = document.AddPage();
                                page.Size = PdfSharpCore.PageSize.A4;
                                gfx = XGraphics.FromPdfPage(page);
                                pageNumber = document.PageCount;
                                y = marginTop;

                                // Draw table header on new page
                                DrawTableHeader();
                            }

                            // Draw the item row
                            double cx = marginLeft;
                            gfx.DrawString(it.Name ?? string.Empty, tableFont, XBrushes.Black, new XRect(cx, y + 4, colNameW, rowHeight), XStringFormats.TopLeft);
                            cx += colNameW;
                            gfx.DrawString(it.Quantity.ToString(), tableFont, XBrushes.Black, new XRect(cx, y + 4, colQtyW, rowHeight), XStringFormats.TopLeft);
                            cx += colQtyW;
                            gfx.DrawString(it.Price.ToString("N2"), tableFont, XBrushes.Black, new XRect(cx, y + 4, colPriceW, rowHeight), XStringFormats.TopLeft);
                            cx += colPriceW;
                            var total = it.Price * it.Quantity;
                            gfx.DrawString(total.ToString("N2"), tableFont, XBrushes.Black, new XRect(cx, y + 4, colTotalW, rowHeight), XStringFormats.TopLeft);

                            grandTotal += total;
                            y += rowHeight;
                        }

                        // After items, ensure there is room for grand total; if not, new page
                        if (y + 30 + marginBottom > pageHeight)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PdfSharpCore.PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            y = marginTop;
                        }

                        y += 10;
                        gfx.DrawString("Grand Total:", labelFont, XBrushes.Black, new XPoint(marginLeft + usableWidth - 200, y));
                        gfx.DrawString(grandTotal.ToString("N2"), valueFont, XBrushes.Black, new XPoint(marginLeft + usableWidth - 80, y));
                    }
                    finally
                    {
                        // Dispose gfx for this job's last page before moving to next job
                        try { gfx?.Dispose(); } catch { /* ignore */ }
                    }
                }

                // Ensure output directory exists
                var outDir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                using var outStream = File.Create(filePath);
                document.Save(outStream);
            });
        }
    }
}