using System;
using System.Collections.Generic;
using System.Linq;

namespace GMSApp.Models
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

    public class QuotationItem
    {
        public int Id { get; set; }
        public int RepairQuotationId { get; set; } // FK

        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Computed
        public decimal Total => Quantity * UnitPrice;
    }

    public enum QuotationStatus
    {
        Draft,
        Sent,
        Approved,
        Rejected,
        ConvertedToJobOrder
    }
}