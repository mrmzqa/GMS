using GMSApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace GMSApp.Services
{
    public class InvoiceService
    {
        public void GenerateInvoice(ServiceJob job, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header()
                        .Text($"Invoice - Job ID: {job.JobId}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .Column(col =>
                        {
                            col.Item().Text($"Vehicle ID: {job.VehicleId}");
                            col.Item().Text($"Issue: {job.ReportedIssue}");
                            col.Item().Text($"Diagnosis: {job.Diagnosis ?? "N/A"}");
                            col.Item().Text($"Fixes: {job.FixesPerformed ?? "N/A"}");
                            col.Item().Text($"Assigned Worker ID: {job.AssignedWorkerId}");
                            col.Item().Text($"Start Date: {job.StartDate}");
                            col.Item().Text($"End Date: {job.EndDate?.ToString() ?? "N/A"}");
                            col.Item().Text($"Total Cost: ${job.Cost:N2}");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated: {DateTime.UtcNow}");
                });
            })
            .GeneratePdf(filePath);
        }
    }
}