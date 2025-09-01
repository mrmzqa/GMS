using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GMSApp.Models.job;
using GMSApp.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace GMSApp.Repositories.Pdf
{
    /// <summary>
    /// A self-contained job-order PDF generator.
    /// Implements the application's generic PDF generator interface:
    ///     public interface IGenericPdfGenerator<T> { Task GeneratePdfAsync(IEnumerable<T> models, string filePath); }
    ///
    /// Usage:
    /// - Register in DI as IGenericPdfGenerator<Joborder>.
    /// - Optionally set the Template property before calling GeneratePdfAsync to control header/footer and multilingual labels.
    /// - For reliable Arabic rendering you should provide an Arabic-capable font name via Template.ArabicFontFamily (installed on the machine),
    ///   or implement and register a PdfSharpCore font resolver to embed a TTF.
    /// </summary>
    public class JoborderPdfGenerator : IGenericPdfGenerator<Joborder>
    {
        // Simple template holder - no external model files required.
        public class Template
        {
            // Header / Footer (English + Arabic)
            public string HeaderEn { get; set; } = "JOB CARD";
            public string HeaderAr { get; set; } = "بطاقة العمل";

            public string FooterLeftEn { get; set; } = "";
            public string FooterLeftAr { get; set; } = "";
            public string FooterRightEn { get; set; } = "";
            public string FooterRightAr { get; set; } = "";

            // Labels for details and table headers
            public string CustomerEn { get; set; } = "Customer";
            public string CustomerAr { get; set; } = "العميل";

            public string PhoneEn { get; set; } = "Phone";
            public string PhoneAr { get; set; } = "الهاتف";

            public string VehicleEn { get; set; } = "Vehicle No";
            public string VehicleAr { get; set; } = "رقم المركبة";

            public string BrandEn { get; set; } = "Brand";
            public string BrandAr { get; set; } = "الماركة";

            public string ModelEn { get; set; } = "Model";
            public string ModelAr { get; set; } = "الموديل";

            public string OdoEn { get; set; } = "Odometer";
            public string OdoAr { get; set; } = "عداد المسافة";

            public string ItemEn { get; set; } = "Item";
            public string ItemAr { get; set; } = "البند";

            public string QtyEn { get; set; } = "Qty";
            public string QtyAr { get; set; } = "الكمية";

            public string PriceEn { get; set; } = "Price";
            public string PriceAr { get; set; } = "السعر";

            public string TotalEn { get; set; } = "Total";
            public string TotalAr { get; set; } = "الإجمالي";

            public string GrandTotalEn { get; set; } = "Grand Total";
            public string GrandTotalAr { get; set; } = "الإجمالي الكلي";

            // Font family names (must exist on runtime machine). Use an Arabic-capable font for Arabic text.
            public string EnglishFontFamily { get; set; } = "Arial";
            public string ArabicFontFamily { get; set; } = "Tahoma";

            // Optionally a logo for header (bytes)
            public byte[]? Logo { get; set; }
        }

        // Default template (can be replaced by caller)
        public Template TemplateData { get; set; } = new Template();

        public JoborderPdfGenerator()
        {
        }

        // The interface method
        public async Task GeneratePdfAsync(IEnumerable<Joborder> models, string filePath)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            // Run PDF creation on background thread to avoid UI blocking
            await Task.Run(() =>
            {
                using var document = new PdfDocument();

                foreach (var job in models)
                {
                    PdfPage page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    try
                    {
                        // Layout basics
                        double ml = 40, mt = 40, mr = 40, mb = 40;
                        double pageW = page.Width;
                        double pageH = page.Height;
                        double usableW = pageW - ml - mr;
                        double y = mt;

                        // Fonts (use template-provided family names)
                        var enFont = TemplateData.EnglishFontFamily ?? "Arial";
                        var arFont = TemplateData.ArabicFontFamily ?? enFont;

                        var headerFont = new XFont(enFont, 16, XFontStyle.Bold);
                        var labelFontEn = new XFont(enFont, 10, XFontStyle.Bold);
                        var valueFontEn = new XFont(enFont, 10, XFontStyle.Regular);
                        var smallFontEn = new XFont(enFont, 9, XFontStyle.Regular);

                        // Note: For proper Arabic shaping and right-to-left layout you should pre-process Arabic strings using a shaping library
                        // and ensure a font that supports Arabic is available and used (TemplateData.ArabicFontFamily).
                        // Here we simply render Arabic text as-is next to English (e.g. "Customer — العميل").

                        // Header: combined English and Arabic
                        var headerText = $"{TemplateData.HeaderEn}  —  {TemplateData.HeaderAr}";
                        gfx.DrawString(headerText, headerFont, XBrushes.Black, new XRect(ml, y, usableW, 24), XStringFormats.TopCenter);
                        y += 28;

                        // Optional logo on header right
                        double logoW = 90, logoH = 50;
                        if (TemplateData.Logo != null && TemplateData.Logo.Length > 0)
                        {
                            try
                            {
                                using var msLogo = new MemoryStream(TemplateData.Logo);
                                using var logoImg = XImage.FromStream(() => msLogo);
                                double lx = ml + usableW - logoW;
                                double ly = mt;
                                gfx.DrawImage(logoImg, lx, ly, logoW, logoH);
                            }
                            catch
                            {
                                // ignore logo errors
                            }
                        }

                        // Draw details in "LabelEn — LabelAr : Value" format
                        void DrawDetail(string labelEn, string labelAr, string? value)
                        {
                            var combinedLabel = string.IsNullOrWhiteSpace(labelAr) ? labelEn : $"{labelEn} — {labelAr}";
                            gfx.DrawString(combinedLabel + ":", labelFontEn, XBrushes.Black, new XPoint(ml, y));
                            gfx.DrawString(value ?? string.Empty, valueFontEn, XBrushes.Black, new XPoint(ml + 160, y));
                            y += 18;
                        }

                        DrawDetail(TemplateData.CustomerEn, TemplateData.CustomerAr, job.CustomerName);
                        DrawDetail(TemplateData.PhoneEn, TemplateData.PhoneAr, job.Phonenumber);
                        DrawDetail(TemplateData.VehicleEn, TemplateData.VehicleAr, job.VehicleNumber);
                        DrawDetail(TemplateData.BrandEn, TemplateData.BrandAr, job.Brand);
                        DrawDetail(TemplateData.ModelEn, TemplateData.ModelAr, job.Model);
                        DrawDetail(TemplateData.OdoEn, TemplateData.OdoAr, job.OdoNumber?.ToString());

                        y += 8;

                        // Optional job front image at right of details
                        double imgW = 120, imgH = 90;
                        if (job.F != null && job.F.Length > 0)
                        {
                            try
                            {
                                using var ms = new MemoryStream(job.F);
                                using var ximg = XImage.FromStream(() => ms);
                                double ix = ml + usableW - imgW;
                                double iy = mt + 28; // below header
                                gfx.DrawImage(ximg, ix, iy, imgW, imgH);
                                if (y < iy + imgH) y = iy + imgH + 8;
                            }
                            catch
                            {
                                // ignore image errors
                            }
                        }

                        // Items table - bordered
                        var items = job.Items?.ToList() ?? new List<ItemRow>();
                        double colNameW = usableW * 0.55;
                        double colQtyW = usableW * 0.12;
                        double colPriceW = usableW * 0.16;
                        double colTotalW = usableW * 0.17;
                        double rowH = 22;

                        // Draw table header (using combined labels)
                        void DrawTableHeader()
                        {
                            double x = ml;
                            var headerRect = new XRect(x, y, usableW, rowH);
                            gfx.DrawRectangle(XBrushes.LightGray, headerRect);

                            gfx.DrawRectangle(XPens.Black, x, y, colNameW, rowH);
                            gfx.DrawString($"{TemplateData.ItemEn} — {TemplateData.ItemAr}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colNameW, rowH), XStringFormats.TopLeft);
                            x += colNameW;

                            gfx.DrawRectangle(XPens.Black, x, y, colQtyW, rowH);
                            gfx.DrawString($"{TemplateData.QtyEn} — {TemplateData.QtyAr}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colQtyW, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            gfx.DrawRectangle(XPens.Black, x, y, colPriceW, rowH);
                            gfx.DrawString($"{TemplateData.PriceEn} — {TemplateData.PriceAr}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colPriceW, rowH), XStringFormats.TopLeft);
                            x += colPriceW;

                            gfx.DrawRectangle(XPens.Black, x, y, colTotalW, rowH);
                            gfx.DrawString($"{TemplateData.TotalEn} — {TemplateData.TotalAr}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colTotalW, rowH), XStringFormats.TopLeft);
                            y += rowH;
                        }

                        DrawTableHeader();

                        decimal grandTotal = 0m;

                        // Render items with simple pagination
                        foreach (var it in items)
                        {
                            if (y + rowH + mb > pageH)
                            {
                                // not enough room -> new page, redraw header
                                gfx.Dispose();
                                page = document.AddPage();
                                page.Size = PdfSharpCore.PageSize.A4;
                                gfx = XGraphics.FromPdfPage(page);
                                y = mt;

                                // Re-draw header title on continuation page
                                var contTitle = $"{TemplateData.HeaderEn} — {TemplateData.HeaderAr} (cont.)";
                                gfx.DrawString(contTitle, headerFont, XBrushes.Black, new XRect(ml, y, usableW, 24), XStringFormats.TopCenter);
                                y += 28;

                                DrawTableHeader();
                            }

                            double x = ml;

                            // Name cell
                            var nameRect = new XRect(x, y, colNameW, rowH);
                            gfx.DrawRectangle(XPens.Black, nameRect);
                            gfx.DrawString(it.Name ?? string.Empty, valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colNameW - 8, rowH), XStringFormats.TopLeft);
                            x += colNameW;

                            // Qty
                            var qtyRect = new XRect(x, y, colQtyW, rowH);
                            gfx.DrawRectangle(XPens.Black, qtyRect);
                            gfx.DrawString(it.Quantity.ToString(), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colQtyW - 8, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            // Price
                            var priceRect = new XRect(x, y, colPriceW, rowH);
                            gfx.DrawRectangle(XPens.Black, priceRect);
                            gfx.DrawString(it.Price.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colPriceW - 8, rowH), XStringFormats.TopLeft);
                            x += colPriceW;

                            // Total
                            var total = it.Price * it.Quantity;
                            var totalRect = new XRect(x, y, colTotalW, rowH);
                            gfx.DrawRectangle(XPens.Black, totalRect);
                            gfx.DrawString(total.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colTotalW - 8, rowH), XStringFormats.TopLeft);

                            grandTotal += total;
                            y += rowH;
                        }

                        // Ensure room for grand total
                        if (y + 30 + mb > pageH)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PdfSharpCore.PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            y = mt;
                        }

                        y += 10;
                        var gtLabel = $"{TemplateData.GrandTotalEn} — {TemplateData.GrandTotalAr}";
                        gfx.DrawString(gtLabel + ":", labelFontEn, XBrushes.Black, new XPoint(ml + usableW - 240, y));
                        gfx.DrawString(grandTotal.ToString("N2"), valueFontEn, XBrushes.Black, new XPoint(ml + usableW - 80, y));

                        // Footer (draw at bottom). Combined English/Arabic
                        var footerLeft = string.IsNullOrWhiteSpace(TemplateData.FooterLeftAr)
                            ? TemplateData.FooterLeftEn
                            : $"{TemplateData.FooterLeftEn} — {TemplateData.FooterLeftAr}";
                        var footerRight = string.IsNullOrWhiteSpace(TemplateData.FooterRightAr)
                            ? TemplateData.FooterRightEn
                            : $"{TemplateData.FooterRightEn} — {TemplateData.FooterRightAr}";

                        if (!string.IsNullOrWhiteSpace(footerLeft))
                        {
                            gfx.DrawString(footerLeft, smallFontEn, XBrushes.Gray, new XRect(ml, pageH - mb + 8, usableW / 2, 20), XStringFormats.TopLeft);
                        }

                        if (!string.IsNullOrWhiteSpace(footerRight))
                        {
                            gfx.DrawString(footerRight, smallFontEn, XBrushes.Gray, new XRect(ml, pageH - mb + 8, usableW, 20), XStringFormats.TopRight);
                        }
                    }
                    finally
                    {
                        // dispose gfx for the page
                        try { gfx?.Dispose(); } catch { /* ignore */ }
                    }
                }

                // Save document to disk
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var stream = File.Create(filePath);
                document.Save(stream);
            });
        }
    }

    // Minimal generic interface used by the app (if not already present in your codebase)
    // Remove this if your project already declares the interface elsewhere.
    public interface IGenericPdfGenerator<T>
    {
        Task GeneratePdfAsync(IEnumerable<T> models, string filePath);
    }
}