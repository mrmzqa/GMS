// PurchaseOrderPdfGenerator.cs
using GMSApp.Data;
using GMSApp.Models;
using GMSApp.Models.purchase;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GMSApp.Repositories.Pdf
{
    public class PurchaseOrderPdfGenerator : IGenericPdfGenerator<PurchaseOrder>
    {
        private readonly AppDbContext _context;
        public class Template
        {
            public string HeaderEn { get; set; } = "PURCHASE ORDER";
            public string HeaderAr { get; set; } = "أمر شراء";

            public string FooterLeftEn { get; set; } = "";
            public string FooterLeftAr { get; set; } = "";
            public string FooterRightEn { get; set; } = "";
            public string FooterRightAr { get; set; } = "";

            public string PoNumberEn { get; set; } = "PO #";
            public string PoNumberAr { get; set; } = "رقم الطلب";

            public string DateEn { get; set; } = "Date";
            public string DateAr { get; set; } = "التاريخ";

            public string VendorEn { get; set; } = "Vendor";
            public string VendorAr { get; set; } = "المورد";

            public string VendorIdEn { get; set; } = "Vendor ID";
            public string VendorIdAr { get; set; } = "معرف المورد";

            public string PaymentEn { get; set; } = "Payment Method";
            public string PaymentAr { get; set; } = "طريقة الدفع";

            public string BankEn { get; set; } = "Bank";
            public string BankAr { get; set; } = "البنك";

            public string IBANEn { get; set; } = "IBAN";
            public string IBANAr { get; set; } = "آيبان";

            public string ExpectedDeliveryEn { get; set; } = "Expected Delivery";
            public string ExpectedDeliveryAr { get; set; } = "تاريخ التسليم المتوقع";

            public string DeliveryLocationEn { get; set; } = "Delivery Location";
            public string DeliveryLocationAr { get; set; } = "مكان التسليم";

            public string NotesEn { get; set; } = "Notes";
            public string NotesAr { get; set; } = "ملاحظات";

            public string ItemEn { get; set; } = "Item";
            public string ItemAr { get; set; } = "البند";

            public string PartEn { get; set; } = "Part #";
            public string PartAr { get; set; } = "رقم القطعة";

            public string UnitEn { get; set; } = "Unit";
            public string UnitAr { get; set; } = "الوحدة";

            public string QtyEn { get; set; } = "Qty";
            public string QtyAr { get; set; } = "الكمية";

            public string UnitPriceEn { get; set; } = "Unit Price";
            public string UnitPriceAr { get; set; } = "سعر الوحدة";

            public string LineTotalEn { get; set; } = "Line Total";
            public string LineTotalAr { get; set; } = "المجموع";

            public string SubTotalEn { get; set; } = "SubTotal";
            public string SubTotalAr { get; set; } = "الإجمالي الفرعي";

            public string DiscountEn { get; set; } = "Discount";
            public string DiscountAr { get; set; } = "الخصم";

            public string TaxEn { get; set; } = "Tax";
            public string TaxAr { get; set; } = "الضريبة";

            public string GrandTotalEn { get; set; } = "Grand Total";
            public string GrandTotalAr { get; set; } = "الإجمالي الكلي";

            public string EnglishFontFamily { get; set; } = "Arial";
            public string ArabicFontFamily { get; set; } = "Arial";

            public byte[]? Logo { get; set; }

            public byte[]? HeaderImage { get; set; }

            public byte[]? FooterImage { get; set; }
        }

        public Template TemplateData { get; set; } = new Template();

        public PurchaseOrderPdfGenerator( AppDbContext context) 
        {

            _context = context;
        }

        // Arabic shaping forms (same approach used in JoborderPdfGenerator)
        static readonly Dictionary<char, (char isolated, char initial, char medial, char final)> ArabicForms = new()
        {
            ['\u0621'] = ('\uFE80', '\uFE80', '\uFE80', '\uFE80'),
            ['\u0622'] = ('\uFE81', '\uFE81', '\uFE82', '\uFE82'),
            ['\u0623'] = ('\uFE83', '\uFE83', '\uFE84', '\uFE84'),
            ['\u0624'] = ('\uFE85', '\uFE85', '\uFE86', '\uFE86'),
            ['\u0625'] = ('\uFE87', '\uFE87', '\uFE88', '\uFE88'),
            ['\u0626'] = ('\uFE89', '\uFE8B', '\uFE8C', '\uFE8A'),
            ['\u0627'] = ('\uFE8D', '\uFE8D', '\uFE8D', '\uFE8E'),
            ['\u0628'] = ('\uFE8F', '\uFE91', '\uFE92', '\uFE90'),
            ['\u0629'] = ('\uFE93', '\uFE93', '\uFE94', '\uFE94'),
            ['\u062A'] = ('\uFE95', '\uFE97', '\uFE98', '\uFE96'),
            ['\u062B'] = ('\uFE99', '\uFE9B', '\uFE9C', '\uFE9A'),
            ['\u062C'] = ('\uFE9D', '\uFE9F', '\uFEA0', '\uFE9E'),
            ['\u062D'] = ('\uFEA1', '\uFEA3', '\uFEA4', '\uFEA2'),
            ['\u062E'] = ('\uFEA5', '\uFEA7', '\uFEA8', '\uFEA6'),
            ['\u062F'] = ('\uFEA9', '\uFEA9', '\uFEAA', '\uFEAA'),
            ['\u0630'] = ('\uFEAB', '\uFEAB', '\uFEAC', '\uFEAC'),
            ['\u0631'] = ('\uFEAD', '\uFEAD', '\uFEAE', '\uFEAE'),
            ['\u0632'] = ('\uFEAF', '\uFEAF', '\uFEB0', '\uFEB0'),
            ['\u0633'] = ('\uFEB1', '\uFEB3', '\uFEB4', '\uFEB2'),
            ['\u0634'] = ('\uFEB5', '\uFEB7', '\uFEB8', '\uFEB6'),
            ['\u0635'] = ('\uFEB9', '\uFEBB', '\uFEBC', '\uFEBA'),
            ['\u0636'] = ('\uFEBD', '\uFEBF', '\uFEC0', '\uFEBE'),
            ['\u0637'] = ('\uFEC1', '\uFEC3', '\uFEC4', '\uFEC2'),
            ['\u0638'] = ('\uFEC5', '\uFEC7', '\uFEC8', '\uFEC6'),
            ['\u0639'] = ('\uFEC9', '\uFECB', '\uFECC', '\uFECA'),
            ['\u063A'] = ('\uFECD', '\uFECF', '\uFED0', '\uFECE'),
            ['\u0640'] = ('\u0640', '\u0640', '\u0640', '\u0640'),
            ['\u0641'] = ('\uFED1', '\uFED3', '\uFED4', '\uFED2'),
            ['\u0642'] = ('\uFED5', '\uFED7', '\uFED8', '\uFED6'),
            ['\u0643'] = ('\uFED9', '\uFEDB', '\uFEDC', '\uFEDA'),
            ['\u0644'] = ('\uFEDD', '\uFEDF', '\uFEE0', '\uFEDE'),
            ['\u0645'] = ('\uFEE1', '\uFEE3', '\uFEE4', '\uFEE2'),
            ['\u0646'] = ('\uFEE5', '\uFEE7', '\uFEE8', '\uFEE6'),
            ['\u0647'] = ('\uFEE9', '\uFEEB', '\uFEEC', '\uFEEA'),
            ['\u0648'] = ('\uFEED', '\uFEED', '\uFEEE', '\uFEEE'),
            ['\u0649'] = ('\uFEEF', '\uFEF3', '\uFEF4', '\uFEF0'),
            ['\u064A'] = ('\uFEF1', '\uFEF3', '\uFEF4', '\uFEF2'),
        };

        static bool IsRightJoining(char c)
        {
            var nonJoiningLetters = new HashSet<char> { '\u0627', '\u062F', '\u0630', '\u0631', '\u0632', '\u0648', '\u0629' };
            return ArabicForms.ContainsKey(c) && !nonJoiningLetters.Contains(c);
        }

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

            Array.Reverse(shaped);
            return new string(shaped);
        }

        public async Task GeneratePdfAsync(IEnumerable<PurchaseOrder> models, string filePath)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            await Task.Run(() =>
            {
                using var document = new PdfDocument();

                foreach (var po in models)
                {
                    PdfPage page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    try
                    {
                        double ml = 36, mt = 36, mr = 36, mb = 36;
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
                            gfx.DrawString(value ?? string.Empty, valueFontEn, XBrushes.Black, new XPoint(ml + 180, y));
                            y += 18;
                        }

                        DrawDetail(TemplateData.PoNumberEn, TemplateData.PoNumberAr, po.PONumber);
                        DrawDetail(TemplateData.DateEn, TemplateData.DateAr, po.Date.ToString("yyyy-MM-dd"));
                        DrawDetail(TemplateData.VendorEn, TemplateData.VendorAr, po.Vendor.Id.ToString());
                        DrawDetail(TemplateData.VendorIdEn, TemplateData.VendorIdAr, po.VendorId?.ToString());
                        DrawDetail(TemplateData.PaymentEn, TemplateData.PaymentAr, po.BankName);
                        DrawDetail(TemplateData.BankEn, TemplateData.BankAr, po.BankName);
                        DrawDetail(TemplateData.IBANEn, TemplateData.IBANAr, po.IBAN);
                        DrawDetail(TemplateData.ExpectedDeliveryEn, TemplateData.ExpectedDeliveryAr, po.ExpectedDeliveryDate?.ToString("yyyy-MM-dd"));
                        DrawDetail(TemplateData.DeliveryLocationEn, TemplateData.DeliveryLocationAr, po.DeliveryLocation);

                        y += 6;
                        // Notes
                        if (!string.IsNullOrWhiteSpace(po.Notes))
                        {
                            gfx.DrawString($"{TemplateData.NotesEn} — {ShapeArabic(TemplateData.NotesAr)}:", labelFontEn, XBrushes.Black, new XPoint(ml, y));
                            y += 14;
                            var notesRect = new XRect(ml, y, usableW, 40);
                            gfx.DrawString(po.Notes ?? "", valueFontEn, XBrushes.Black, notesRect, XStringFormats.TopLeft);
                            y += 44;
                        }

                        // Items table
                        var items = po.Lines?.ToList() ?? new List<PurchaseOrderLine>();
                        double colDescW = usableW * 0.36;
                        double colPartW = usableW * 0.14;
                        double colUnitW = usableW * 0.10;
                        double colQtyW = usableW * 0.10;
                        double colUnitPriceW = usableW * 0.15;
                        double colLineTotalW = usableW * 0.15;
                        double rowH = 20;

                        void DrawTableHeader()
                        {
                            double x = ml;
                            gfx.DrawRectangle(XBrushes.LightGray, new XRect(x, y, usableW, rowH));

                            gfx.DrawRectangle(XPens.Black, x, y, colDescW, rowH);
                            gfx.DrawString($"{TemplateData.ItemEn} — {ShapeArabic(TemplateData.ItemAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colDescW, rowH), XStringFormats.TopLeft);
                            x += colDescW;

                            gfx.DrawRectangle(XPens.Black, x, y, colPartW, rowH);
                            gfx.DrawString($"{TemplateData.PartEn} — {ShapeArabic(TemplateData.PartAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colPartW, rowH), XStringFormats.TopLeft);
                            x += colPartW;

                            gfx.DrawRectangle(XPens.Black, x, y, colUnitW, rowH);
                            gfx.DrawString($"{TemplateData.UnitEn} — {ShapeArabic(TemplateData.UnitAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colUnitW, rowH), XStringFormats.TopLeft);
                            x += colUnitW;

                            gfx.DrawRectangle(XPens.Black, x, y, colQtyW, rowH);
                            gfx.DrawString($"{TemplateData.QtyEn} — {ShapeArabic(TemplateData.QtyAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colQtyW, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            gfx.DrawRectangle(XPens.Black, x, y, colUnitPriceW, rowH);
                            gfx.DrawString($"{TemplateData.UnitPriceEn} — {ShapeArabic(TemplateData.UnitPriceAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colUnitPriceW, rowH), XStringFormats.TopLeft);
                            x += colUnitPriceW;

                            gfx.DrawRectangle(XPens.Black, x, y, colLineTotalW, rowH);
                            gfx.DrawString($"{TemplateData.LineTotalEn} — {ShapeArabic(TemplateData.LineTotalAr)}", labelFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colLineTotalW, rowH), XStringFormats.TopLeft);
                            y += rowH;
                        }

                        DrawTableHeader();

                        decimal computedSubTotal = 0m;
                        foreach (var line in items)
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

                            gfx.DrawRectangle(XPens.Black, x, y, colDescW, rowH);
                            gfx.DrawString(line.Description ?? "", valueFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colDescW - 8, rowH), XStringFormats.TopLeft);
                            x += colDescW;

                            gfx.DrawRectangle(XPens.Black, x, y, colPartW, rowH);
                            gfx.DrawString(line.PartNumber ?? "", valueFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colPartW - 8, rowH), XStringFormats.TopLeft);
                            x += colPartW;

                            gfx.DrawRectangle(XPens.Black, x, y, colUnitW, rowH);
                            gfx.DrawString(line.Unit ?? "", valueFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colUnitW - 8, rowH), XStringFormats.TopLeft);
                            x += colUnitW;

                            gfx.DrawRectangle(XPens.Black, x, y, colQtyW, rowH);
                            gfx.DrawString(line.Quantity.ToString(), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colQtyW - 8, rowH), XStringFormats.TopLeft);
                            x += colQtyW;

                            gfx.DrawRectangle(XPens.Black, x, y, colUnitPriceW, rowH);
                            gfx.DrawString(line.UnitPrice.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colUnitPriceW - 8, rowH), XStringFormats.TopLeft);
                            x += colUnitPriceW;

                            decimal lineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
                            gfx.DrawRectangle(XPens.Black, x, y, colLineTotalW, rowH);
                            gfx.DrawString(lineTotal.ToString("N2"), valueFontEn, XBrushes.Black, new XRect(x + 4, y + 3, colLineTotalW - 8, rowH), XStringFormats.TopLeft);
                            y += rowH;

                            computedSubTotal += lineTotal;
                        }

                        // totals block
                        if (y + 80 + mb > pageH)
                        {
                            gfx.Dispose();
                            page = document.AddPage();
                            page.Size = PdfSharpCore.PageSize.A4;
                            gfx = XGraphics.FromPdfPage(page);
                            y = mt;
                        }

                        y += 10;
                        double totalsX = ml + usableW - 260;
                        void DrawTotalsRow(string labelEn, string labelAr, string value)
                        {
                            var lbl = string.IsNullOrWhiteSpace(labelAr) ? labelEn : $"{labelEn} — {ShapeArabic(labelAr)}";
                            gfx.DrawString(lbl + ":", labelFontEn, XBrushes.Black, new XPoint(totalsX, y));
                            gfx.DrawString(value, valueFontEn, XBrushes.Black, new XPoint(totalsX + 180, y));
                            y += 18;
                        }

                        DrawTotalsRow(TemplateData.SubTotalEn, TemplateData.SubTotalAr, computedSubTotal.ToString("N2"));
                        DrawTotalsRow(TemplateData.DiscountEn, TemplateData.DiscountAr, po.Discount.ToString("N2"));
                        DrawTotalsRow(TemplateData.TaxEn, TemplateData.TaxAr, po.Tax.ToString("N2"));

                        var grand = Math.Round(computedSubTotal - po.Discount + po.Tax, 2);
                        DrawTotalsRow(TemplateData.GrandTotalEn, TemplateData.GrandTotalAr, grand.ToString("N2"));

                        // footer
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