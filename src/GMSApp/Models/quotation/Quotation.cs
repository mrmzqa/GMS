using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.quotation
{
    public class Quotation
    {
        public int Id { get; set; }

        // Quotation Info
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime DateIssued { get; set; }
        public DateTime ValidUntil { get; set; }

        // Customer & Vehicle
        public string CustomerName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleYear { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;

        // Navigation Property
        public List<QuotationItem> Items { get; set; } = new();

        // Financial Settings
        public decimal LabourCharges { get; set; }
        public decimal Discount { get; set; }   // flat discount
        public decimal VatRate { get; set; } = 0.05m; // 5% default for Qatar

        // Computed Properties
        public decimal SubTotal => Items.Sum(i => i.Total) + LabourCharges;
        public decimal VatAmount => (SubTotal - Discount) * VatRate;
        public decimal GrandTotal => (SubTotal - Discount) + VatAmount;

        // Status
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    }
}
