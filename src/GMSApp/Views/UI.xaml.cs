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
    

    // Minimal generic interface used by the app (if not already present in your codebase)
    // Remove this if your project already declares the interface elsewhere.
    
}