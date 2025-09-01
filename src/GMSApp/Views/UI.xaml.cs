// ViewModels/Job/JoborderViewModel.cs (only relevant parts shown)
using GMSApp.Models.Pdf;
using GMSApp.Repositories.Pdf;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// add field
private readonly IJoborderPdfGenerator _jobPdfGenerator;

// constructor signature update to receive generator (add param)
public JoborderViewModel(IRepository<Joborder> jobRepo,
                         IFileRepository fileRepo,
                         IGenericPdfGenerator<Joborder> pdfGenerator, // keep if used elsewhere
                         IJoborderPdfGenerator jobPdfGenerator)
{
    _jobRepo = jobRepo ?? throw new ArgumentNullException(nameof(jobRepo));
    _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
    _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
    _jobPdfGenerator = jobPdfGenerator ?? throw new ArgumentNullException(nameof(jobPdfGenerator));

    _ = LoadJobordersAsync();
}

// PrintAsync -> use jobPdfGenerator
[RelayCommand(CanExecute = nameof(CanModify))]
public async Task PrintAsync()
{
    if (SelectedJoborder == null) return;

    try
    {
        // Build a model copy from UI (ensures Items reflect UI)
        var model = BuildJoborderFromUi(SelectedJoborder);

        // Create a simple template (in real app you might load this from DB or settings)
        var template = new PdfTemplate
        {
            HeaderTitle = new LabelText { En = "JOB CARD", Ar = "بطاقة العمل" },
            FooterLeft = new LabelText { En = "Company Name", Ar = "اسم الشركة" },
            FooterRight = new LabelText { En = "Phone: 123-456", Ar = "هاتف: 123-456" },
            // Optionally set fonts that exist on target machine
            EnglishFontFamily = "Arial",
            ArabicFontFamily = "Tahoma"
        };

        var outPath = Path.Combine(Path.GetTempPath(), $"JobCard_{model.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        // Use the job-specific generator that accepts a template (header/footer + labels)
        await _jobPdfGenerator.GeneratePdfAsync(new[] { model }, outPath, template);

        // Open generated PDF
        var psi = new ProcessStartInfo(outPath) { UseShellExecute = true };
        Process.Start(psi);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
// In your ConfigureServices block:
services.AddTransient<Repositories.Pdf.IJoborderPdfGenerator, Repositories.Pdf.JoborderPdfGenerator>();

// Keep or add the concrete mapping for generic generator if required elsewhere
services.AddTransient(typeof(IGenericPdfGenerator<>), typeof(GenericPdfGenerator<>));

// Repositories/Pdf/JoborderPdfGenerator.cs
using GMSApp.Models.job;
using GMSApp.Models.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GMSApp.Repositories.Pdf
{
    public class JoborderPdfGenerator : IJoborderPdfGenerator
    {
        public async Task GeneratePdfAsync(IEnumerable<Joborder> jobs, string filePath, PdfTemplate template)
        {
            if (jobs == null) throw new ArgumentNullException(nameof(jobs));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (template == null) throw new ArgumentNullException(nameof(template));

            await Task.Run(() =>
            {
                using var document = new PdfDocument();

                foreach (var job in jobs)
                {
                    // Create first page
                    var page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    try
                    {
                        // Margins and layout
                        double ml = 40, mt = 40, mr = 40, mb = 40;
                        double pageW = page.Width;
                        double pageH = page.Height;
                        double usableW = pageW - ml - mr;
                        double y = mt;

                        // Fonts - use template-specified fonts where possible
                        var enFontName = string.IsNullOrWhiteSpace(template.EnglishFontFamily) ? "Arial" : template.EnglishFontFamily;
                        var arFontName = string.IsNullOrWhiteSpace(template.ArabicFontFamily) ? enFontName : template.ArabicFontFamily;

                        var titleFont = new XFont(enFontName, 16, XFontStyle.Bold);
                        var labelFontEn = new XFont(enFontName, 10, XFontStyle.Bold);
                        var valueFontEn = new XFont(enFontName, 10, XFontStyle.Regular);
                        var smallFontEn = new XFont(enFontName, 9, XFontStyle.Regular);

                        // Arabic fonts (use same size but different family)
                        var labelFontAr = new XFont(arFontName, 10, XFontStyle.Bold);
                        var valueFontAr = new XFont(arFontName, 10, XFontStyle.Regular);

                        // Header (template.HeaderTitle)
                        var headerText = $"{template.HeaderTitle.En}  —  {template.HeaderTitle.Ar}";
                        gfx.DrawString(headerText, titleFont, XBrushes.Black, new XRect(ml, y, usableW, 24), XStringFormats.TopCenter);
                        y += 28;

                        // Logo (if provided) - draw at right top
                        double logoW = 100, logoH = 60;
                        if (template.Logo != null && template.Logo.Length > 0)
                        {
                            try
                            {
                                using var ms = new MemoryStream(template.Logo);
                                using var logoImg = XImage.FromStream(() => ms);
                                double lx = ml + usableW - logoW;
                                double ly = mt;
                                gfx.DrawImage(logoImg, lx, ly, logoW, logoH);
                            }
                            catch
                            {
                                // ignore logo errors
                            }
                        }

                        // Draw details as "LabelEn — LabelAr : Value"
                        void DrawDetail(LabelText label, string? value)
                        {
                            // Print English label then Arabic label in parentheses
                            var labelCombined = label.ToString(); // uses "En — Ar"
                            gfx.DrawString(labelCombined + ":", labelFontEn, XBrushes.Black, new XPoint(ml, y));
                            // Value - try English value then Arabic value if present (for example job.CustomerName may be in both)
                            gfx.DrawString(value ?? string.Empty, valueFontEn, XBrushes.Black, new XPoint(ml + 160, y));
                            y += 18;
                        }

                        DrawDetail(template.CustomerLabel, job.CustomerName);
                        DrawDetail(template.PhoneLabel, job.Phonenumber);
                        DrawDetail(template.VehicleLabel, job.VehicleNumber);
                        DrawDetail(template.BrandLabel, job.Brand);
                        DrawDetail(template.ModelLabel, job.Model);
                        DrawDetail(template.OdoLabel, job.OdoNumber?.ToString());

                        y += 8;

                        // Optional front image display (small) to the right of details if exists
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

                                // ensure y is below the image if it overlaps
                                if (y < iy + imgH) y = iy + imgH + 8;
                            }
                            catch
                            {
                                // ignore image errors
                            }
                        }

                        // Draw items table with borders and pagination
                        // Column widths
                        double colNameW = usableW * 0.55;
                        double colQtyW = usableW * 0.12;
                        double colPriceW = usableW * 0.16;
                        double colTotalW = usableW * 0.17;
                        double rowH = 22;

                        // Draw table header
                        void DrawTableHeader()
                        {
                            double x = ml;
                            // header background
                            var headerRect = new XRect(x, y, usableW, rowH);
                            gfx.DrawRectangle(XBrushes.LightGray, headerRect);

                            gfx.DrawRectangle(XPens.Black, x, y, colNameW, rowH);
                            gfx.DrawString(template.ItemLabel.ToString(), labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colNameW, rowH), XStringFormats.TopLeft);
                            x += colNameW;

                            gfx.DrawRectangle(XPens.Black, x, y, colQtyW, rowH);
                            gfx.DrawString(template.QtyLabel.ToString(), labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colQtyW, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            gfx.DrawRectangle(XPens.Black, x, y, colPriceW, rowH);
                            gfx.DrawString(template.PriceLabel.ToString(), labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colPriceW, rowH), XStringFormats.TopLeft);
                            x += colPriceW;

                            gfx.DrawRectangle(XPens.Black, x, y, colTotalW, rowH);
                            gfx.DrawString(template.TotalLabel.ToString(), labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colTotalW, rowH), XStringFormats.TopLeft);

                            y += rowH;
                        }

                        DrawTableHeader();

                        var items = job.Items?.ToList() ?? new List<GMSApp.Models.ItemRow>();
                        decimal grandTotal = 0m;
                        int currentPageNumber = document.PageCount;

                        foreach (var it in items)
                        {
                            // If next row would overflow page bottom, create new page and redraw header
                            if (y + rowH + mb > pageH)
                            {
                                gfx.Dispose();
                                page = document.AddPage();
                                page.Size = PdfSharpCore.PageSize.A4;
                                gfx = XGraphics.FromPdfPage(page);
                                currentPageNumber = document.PageCount;
                                y = mt;

                                // optional small continuation header
                                var contTitle = $"{template.HeaderTitle.En} — {template.HeaderTitle.Ar} (cont.)";
                                gfx.DrawString(contTitle, titleFont, XBrushes.Black, new XRect(ml, y, usableW, 20), XStringFormats.TopCenter);
                                y += 26;

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
                            var totalVal = it.Quantity * it.Price;
                            var totalRect = new XRect(x, y, colTotalW, rowH);
                            gfx.DrawRectangle(XPens.Black, totalRect);
                            gfx.DrawString(totalVal.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colTotalW - 8, rowH), XStringFormats.TopLeft);

                            grandTotal += totalVal;
                            y += rowH;
                        }

                        // After items, draw grand total (ensure room)
                        if (y + 30 + mb > pageH)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PdfSharpCore.PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            y = mt;
                        }

                        y += 10;
                        var gtLabel = template.GrandTotalLabel.ToString();
                        gfx.DrawString(gtLabel + ":", labelFontEn, XBrushes.Black, new XPoint(ml + usableW - 240, y));
                        gfx.DrawString(grandTotal.ToString("N2"), valueFontEn, XBrushes.Black, new XPoint(ml + usableW - 80, y));

                        // Footer content from template (draw at bottom-left/right if provided)
                        if (!string.IsNullOrWhiteSpace(template.FooterLeft?.En) || !string.IsNullOrWhiteSpace(template.FooterLeft?.Ar))
                        {
                            var footerLeftText = template.FooterLeft.ToString();
                            gfx.DrawString(footerLeftText, smallFontEn, XBrushes.Gray, new XRect(ml, pageH - mb + 8, usableW / 2, 20), XStringFormats.TopLeft);
                        }

                        if (!string.IsNullOrWhiteSpace(template.FooterRight?.En) || !string.IsNullOrWhiteSpace(template.FooterRight?.Ar))
                        {
                            var footerRightText = template.FooterRight.ToString();
                            gfx.DrawString(footerRightText, smallFontEn, XBrushes.Gray, new XRect(ml, pageH - mb + 8, usableW, 20), XStringFormats.TopRight);
                        }
                    }
                    finally
                    {
                        gfx?.Dispose();
                    }
                }

                // Save to disk
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var fs = File.Create(filePath);
                document.Save(fs);
            });
        }
    }
}

// Repositories/Pdf/IJoborderPdfGenerator.cs
using GMSApp.Models.job;
using GMSApp.Models.Pdf;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMSApp.Repositories.Pdf
{
    public interface IJoborderPdfGenerator
    {
        Task GeneratePdfAsync(IEnumerable<Joborder> jobs, string filePath, PdfTemplate template);
    }
}

// Models/Pdf/PdfTemplate.cs
using System;
namespace GMSApp.Models.Pdf
{
    // A simple template that holds header/footer and common labels in both languages
    public class PdfTemplate
    {
        // Optional: small header text (English / Arabic)
        public LabelText HeaderTitle { get; set; } = new LabelText { En = "JOB CARD", Ar = "بطاقة العمل" };

        // Optional logo bytes (e.g., PNG/JPG) to be drawn near the header
        public byte[]? Logo { get; set; }

        // Footer texts
        public LabelText FooterLeft { get; set; } = new LabelText { En = "", Ar = "" };
        public LabelText FooterRight { get; set; } = new LabelText { En = "", Ar = "" };

        // Labels for data fields (customer, phone, ... and table headers)
        public LabelText CustomerLabel { get; set; } = new LabelText { En = "Customer", Ar = "العميل" };
        public LabelText PhoneLabel { get; set; } = new LabelText { En = "Phone", Ar = "الهاتف" };
        public LabelText VehicleLabel { get; set; } = new LabelText { En = "Vehicle No", Ar = "رقم المركبة" };
        public LabelText BrandLabel { get; set; } = new LabelText { En = "Brand", Ar = "الماركة" };
        public LabelText ModelLabel { get; set; } = new LabelText { En = "Model", Ar = "الموديل" };
        public LabelText OdoLabel { get; set; } = new LabelText { En = "Odometer", Ar = "عداد المسافة" };

        public LabelText ItemLabel { get; set; } = new LabelText { En = "Item", Ar = "البند" };
        public LabelText QtyLabel { get; set; } = new LabelText { En = "Qty", Ar = "الكمية" };
        public LabelText PriceLabel { get; set; } = new LabelText { En = "Price", Ar = "السعر" };
        public LabelText TotalLabel { get; set; } = new LabelText { En = "Total", Ar = "الإجمالي" };

        public LabelText GrandTotalLabel { get; set; } = new LabelText { En = "Grand Total", Ar = "الإجمالي الكلي" };

        // Optionally store font family names to use
        public string? EnglishFontFamily { get; set; } = "Arial";
        public string? ArabicFontFamily { get; set; } = "Tahoma"; // choose an Arabic-capable font on target machine
    }
}

// Models/Pdf/LabelText.cs
namespace GMSApp.Models.Pdf
{
    public class LabelText
    {
        // English label
        public string En { get; set; } = string.Empty;

        // Arabic label
        public string Ar { get; set; } = string.Empty;

        public override string ToString()
        {
            // Format: "English — Arabic"
            // You can change formatting to "Arabic (English)" or separate lines.
            return string.IsNullOrWhiteSpace(Ar) ? En : $"{En} — {Ar}";
        }
    }
}