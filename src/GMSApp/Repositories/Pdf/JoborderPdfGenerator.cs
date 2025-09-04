
using GMSApp.Data;
using GMSApp.Models;
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
    public class JoborderPdfGenerator : IGenericPdfGenerator<Joborder>
    {
        private readonly AppDbContext _context;
        public class Template
        {
            public string HeaderEn { get; set; } = "JOB CARD";
            public string HeaderAr { get; set; } = "بطاقة العمل";


            public string FooterLeftEn { get; set; } = "";
            public string FooterLeftAr { get; set; } = "";
            public string FooterRightEn { get; set; } = "";
            public string FooterRightAr { get; set; } = "";

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

            public string EnglishFontFamily { get; set; } = "Arial";
            public string ArabicFontFamily { get; set; } = "Arial"; // Better Arabic support

            public byte[]? Logo { get; set; }

           
            public byte[]? HeaderImage { get; set; } 

            public byte[]? FooterImage { get; set; } 
        }


        public Template TemplateData { get; set; } = new Template();

        public JoborderPdfGenerator( AppDbContext appDbContext) 
        
        { 
        
            _context = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));


        }

        // Arabic Unicode presentation forms (simplified)
        // Arabic letters with their isolated, initial, medial, and final forms.
        // Note: Some letters do not connect to the following letter (non-joining letters).
        static readonly Dictionary<char, (char isolated, char initial, char medial, char final)> ArabicForms = new()
        {
            ['\u0621'] = ('\uFE80', '\uFE80', '\uFE80', '\uFE80'), // Hamza
            ['\u0622'] = ('\uFE81', '\uFE81', '\uFE82', '\uFE82'), // Alef with madda
            ['\u0623'] = ('\uFE83', '\uFE83', '\uFE84', '\uFE84'), // Alef with hamza above
            ['\u0624'] = ('\uFE85', '\uFE85', '\uFE86', '\uFE86'), // Waw with hamza
            ['\u0625'] = ('\uFE87', '\uFE87', '\uFE88', '\uFE88'), // Alef with hamza below
            ['\u0626'] = ('\uFE89', '\uFE8B', '\uFE8C', '\uFE8A'), // Yeh with hamza
            ['\u0627'] = ('\uFE8D', '\uFE8D', '\uFE8D', '\uFE8E'), // Alef
            ['\u0628'] = ('\uFE8F', '\uFE91', '\uFE92', '\uFE90'), // Beh
            ['\u0629'] = ('\uFE93', '\uFE93', '\uFE94', '\uFE94'), // Teh Marbuta (non-connecting)
            ['\u062A'] = ('\uFE95', '\uFE97', '\uFE98', '\uFE96'), // Teh
            ['\u062B'] = ('\uFE99', '\uFE9B', '\uFE9C', '\uFE9A'), // Theh
            ['\u062C'] = ('\uFE9D', '\uFE9F', '\uFEA0', '\uFE9E'), // Jeem
            ['\u062D'] = ('\uFEA1', '\uFEA3', '\uFEA4', '\uFEA2'), // Hah
            ['\u062E'] = ('\uFEA5', '\uFEA7', '\uFEA8', '\uFEA6'), // Khah
            ['\u062F'] = ('\uFEA9', '\uFEA9', '\uFEAA', '\uFEAA'), // Dal (non-connecting)
            ['\u0630'] = ('\uFEAB', '\uFEAB', '\uFEAC', '\uFEAC'), // Thal (non-connecting)
            ['\u0631'] = ('\uFEAD', '\uFEAD', '\uFEAE', '\uFEAE'), // Reh (non-connecting)
            ['\u0632'] = ('\uFEAF', '\uFEAF', '\uFEB0', '\uFEB0'), // Zain (non-connecting)
            ['\u0633'] = ('\uFEB1', '\uFEB3', '\uFEB4', '\uFEB2'), // Seen
            ['\u0634'] = ('\uFEB5', '\uFEB7', '\uFEB8', '\uFEB6'), // Sheen
            ['\u0635'] = ('\uFEB9', '\uFEBB', '\uFEBC', '\uFEBA'), // Sad
            ['\u0636'] = ('\uFEBD', '\uFEBF', '\uFEC0', '\uFEBE'), // Dad
            ['\u0637'] = ('\uFEC1', '\uFEC3', '\uFEC4', '\uFEC2'), // Tah
            ['\u0638'] = ('\uFEC5', '\uFEC7', '\uFEC8', '\uFEC6'), // Zah
            ['\u0639'] = ('\uFEC9', '\uFECB', '\uFECC', '\uFECA'), // Ain
            ['\u063A'] = ('\uFECD', '\uFECF', '\uFED0', '\uFECE'), // Ghain
            ['\u0640'] = ('\u0640', '\u0640', '\u0640', '\u0640'), // Tatweel (connecting)
            ['\u0641'] = ('\uFED1', '\uFED3', '\uFED4', '\uFED2'), // Feh
            ['\u0642'] = ('\uFED5', '\uFED7', '\uFED8', '\uFED6'), // Qaf
            ['\u0643'] = ('\uFED9', '\uFEDB', '\uFEDC', '\uFEDA'), // Kaf
            ['\u0644'] = ('\uFEDD', '\uFEDF', '\uFEE0', '\uFEDE'), // Lam
            ['\u0645'] = ('\uFEE1', '\uFEE3', '\uFEE4', '\uFEE2'), // Meem
            ['\u0646'] = ('\uFEE5', '\uFEE7', '\uFEE8', '\uFEE6'), // Noon
            ['\u0647'] = ('\uFEE9', '\uFEEB', '\uFEEC', '\uFEEA'), // Heh
            ['\u0648'] = ('\uFEED', '\uFEED', '\uFEEE', '\uFEEE'), // Waw (non-connecting)
            ['\u0649'] = ('\uFEEF', '\uFEF3', '\uFEF4', '\uFEF0'), // Alef Maksura
            ['\u064A'] = ('\uFEF1', '\uFEF3', '\uFEF4', '\uFEF2'), // Yeh
        };


        // Check if letter connects to next letter
        static bool IsRightJoining(char c)
        {
            // Letters that do not join to the next letter
            var nonJoiningLetters = new HashSet<char> { '\u0627', '\u062F', '\u0630', '\u0631', '\u0632', '\u0648', '\u0629' };
            return ArabicForms.ContainsKey(c) && !nonJoiningLetters.Contains(c);
        }



        // Basic Arabic shaping function
        string ShapeArabic(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var chars = input.ToCharArray();
            var shaped = new char[chars.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                char prev = (i > 0) ? chars[i - 1] : '\0';
                char curr = chars[i];
                char next = (i < chars.Length - 1) ? chars[i + 1] : '\0';

                bool prevJoin = IsRightJoining(prev);
                bool nextJoin = IsRightJoining(curr);

                if (!ArabicForms.TryGetValue(curr, out var forms))
                {
                    // Not Arabic letter, keep as is
                    shaped[i] = curr;
                    continue;
                }

                bool connectPrev = prevJoin && ArabicForms.ContainsKey(curr);
                bool connectNext = ArabicForms.ContainsKey(next) && nextJoin;

                if (connectPrev && connectNext)
                    shaped[i] = forms.medial;
                else if (connectPrev)
                    shaped[i] = forms.final;
                else if (connectNext)
                    shaped[i] = forms.initial;
                else
                    shaped[i] = forms.isolated;
            }

            // Arabic is right-to-left, so reverse shaped string to display correctly
            Array.Reverse(shaped);
            return new string(shaped);
        }


        public async Task GeneratePdfAsync(IEnumerable<Joborder> models, string filePath)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

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
                        double ml = 40, mt = 40, mr = 40, mb = 40;
                        double pageW = page.Width;
                        double pageH = page.Height;
                        double usableW = pageW - ml - mr;
                        double y = mt;

                        var enFont = TemplateData.EnglishFontFamily ?? "Arial";
                        var arFont = TemplateData.ArabicFontFamily ?? enFont;

                        var headerFont = new XFont(enFont, 16, XFontStyle.Bold);
                        var labelFontEn = new XFont(enFont, 10, XFontStyle.Bold);
                        var valueFontEn = new XFont(enFont, 10, XFontStyle.Regular);
                        var smallFontEn = new XFont(enFont, 9, XFontStyle.Regular);

                        var headerText = $"{TemplateData.HeaderEn} — {ShapeArabic(TemplateData.HeaderAr)}";
                        gfx.DrawString(headerText, headerFont, XBrushes.Black, new XRect(ml, y, usableW, 24), XStringFormats.TopCenter);
                        y += 28;

                        // Logo
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
                            catch { }
                        }

                        void DrawDetail(string labelEn, string labelAr, string? value)
                        {
                            var combinedLabel = string.IsNullOrWhiteSpace(labelAr)
                                ? labelEn
                                : $"{labelEn} — {ShapeArabic(labelAr)}";

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
                        //header
                        var HeaderImage = TemplateData.HeaderImage;
                        var FooterImage = TemplateData.FooterImage;
                        HeaderImage = _context.Garages.FirstOrDefault().HeaderFile;
                        FooterImage = _context.Garages.FirstOrDefault().FooterFile;

                        if (HeaderImage != null && HeaderImage.Length > 0)
                        {
                            try
                            {
                                using var msHeader = new MemoryStream(HeaderImage);
                                using var headerImg = XImage.FromStream(() => msHeader);
                                double hh = 60;
                                double hw = headerImg.PixelWidth * hh / headerImg.PixelHeight;
                                double hx = ml + (usableW - hw) / 2;
                                double hy = y;
                                gfx.DrawImage(headerImg, hx, hy, hw, hh);
                                y += hh + 8;
                            }
                            catch { }
                        }


                        //footer
                        if (FooterImage != null && FooterImage.Length > 0)
                        {
                            try
                            {
                                using var msFooter = new MemoryStream(FooterImage);
                                using var footerImg = XImage.FromStream(() => msFooter);
                                double fh = 60;
                                double fw = footerImg.PixelWidth * fh / footerImg.PixelHeight;
                                double fx = ml + (usableW - fw) / 2;
                                double fy = pageH - mb - fh + 20;
                                gfx.DrawImage(footerImg, fx, fy, fw, fh);
                            }
                            catch { }
                        }


                        // Image
                        double imgW = 120, imgH = 90;
                        if (job.F != null && job.F.Length > 0)
                        {
                            try
                            {
                                using var ms = new MemoryStream(job.F);
                                using var ximg = XImage.FromStream(() => ms);
                                double ix = ml + usableW - imgW;
                                double iy = mt + 28;
                                gfx.DrawImage(ximg, ix, iy, imgW, imgH);
                                if (y < iy + imgH) y = iy + imgH + 8;
                            }
                            catch { }
                        }

                        // Items
                        var items = job.Items?.ToList() ?? new List<ItemRow>();
                        double colNameW = usableW * 0.55;
                        double colQtyW = usableW * 0.12;
                        double colPriceW = usableW * 0.16;
                        double colTotalW = usableW * 0.17;
                        double rowH = 22;

                        void DrawTableHeader()
                        {
                            double x = ml;
                            gfx.DrawRectangle(XBrushes.LightGray, new XRect(x, y, usableW, rowH));

                            gfx.DrawRectangle(XPens.Black, x, y, colNameW, rowH);
                            gfx.DrawString($"{TemplateData.ItemEn} | {ShapeArabic(TemplateData.ItemAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colNameW, rowH), XStringFormats.TopLeft);
                            x += colNameW;

                            gfx.DrawRectangle(XPens.Black, x, y, colQtyW, rowH);
                            gfx.DrawString($"{TemplateData.QtyEn} | {ShapeArabic(TemplateData.QtyAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colQtyW, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            gfx.DrawRectangle(XPens.Black, x, y, colPriceW, rowH);
                            gfx.DrawString($"{TemplateData.PriceEn} | {ShapeArabic(TemplateData.PriceAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colPriceW, rowH), XStringFormats.TopLeft);
                            x += colPriceW;

                            gfx.DrawRectangle(XPens.Black, x, y, colTotalW, rowH);
                            gfx.DrawString($"{TemplateData.TotalEn} | {ShapeArabic(TemplateData.TotalAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colTotalW, rowH), XStringFormats.TopLeft);
                            y += rowH;
                        }

                        DrawTableHeader();
                        decimal grandTotal = 0m;

                        foreach (var it in items)
                        {
                            if (y + rowH + mb > pageH)
                            {
                                gfx.Dispose();
                                page = document.AddPage();
                                page.Size = PdfSharpCore.PageSize.A4;
                                gfx = XGraphics.FromPdfPage(page);
                                y = mt;

                                gfx.DrawString($"{TemplateData.HeaderEn} — {ShapeArabic(TemplateData.HeaderAr)} (cont.)", headerFont, XBrushes.Black, new XRect(ml, y, usableW, 24), XStringFormats.TopCenter);
                                y += 28;

                                DrawTableHeader();
                            }

                            double x = ml;

                            gfx.DrawRectangle(XPens.Black, x, y, colNameW, rowH);
                            gfx.DrawString(it.Name ?? "", valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colNameW - 8, rowH), XStringFormats.TopLeft);
                            x += colNameW;

                            gfx.DrawRectangle(XPens.Black, x, y, colQtyW, rowH);
                            gfx.DrawString(it.Quantity.ToString(), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colQtyW - 8, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            gfx.DrawRectangle(XPens.Black, x, y, colPriceW, rowH);
                            gfx.DrawString(it.Price.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colPriceW - 8, rowH), XStringFormats.TopLeft);
                            x += colPriceW;

                            decimal total = it.Price * it.Quantity;
                            gfx.DrawRectangle(XPens.Black, x, y, colTotalW, rowH);
                            gfx.DrawString(total.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 4, colTotalW - 8, rowH), XStringFormats.TopLeft);
                            y += rowH;
                            grandTotal += total;
                        }

                        if (y + 30 + mb > pageH)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PdfSharpCore.PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            y = mt;
                        }

                        y += 10;
                        var gtLabel = $"{TemplateData.GrandTotalEn} — {ShapeArabic(TemplateData.GrandTotalAr)}";
                        gfx.DrawString(gtLabel + ":", labelFontEn, XBrushes.Black, new XPoint(ml + usableW - 240, y));
                        gfx.DrawString(grandTotal.ToString("N2"), valueFontEn, XBrushes.Black, new XPoint(ml + usableW - 80, y));

                        var footerLeft = string.IsNullOrWhiteSpace(TemplateData.FooterLeftAr)
                            ? TemplateData.FooterLeftEn
                            : $"{TemplateData.FooterLeftEn} — {ShapeArabic(TemplateData.FooterLeftAr)}";

                        var footerRight = string.IsNullOrWhiteSpace(TemplateData.FooterRightAr)
                            ? TemplateData.FooterRightEn
                            : $"{TemplateData.FooterRightEn} — {ShapeArabic(TemplateData.FooterRightAr)}";

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
                        try { gfx?.Dispose(); } catch { }
                    }
                }

                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var stream = File.Create(filePath);
                document.Save(stream);
            });
        }
    }

}
