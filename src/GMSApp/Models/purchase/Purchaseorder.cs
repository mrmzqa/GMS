// File: Models/PurchaseOrder.cs
using CommunityToolkit.Mvvm.ComponentModel;
using GMSApp.Models.payment;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models.purchase
{
    public class PurchaseOrder : ObservableObject
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string PONumber { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public int? VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        private decimal _subTotal;
        public decimal SubTotal
        {
            get => _subTotal;
            set => SetProperty(ref _subTotal, value);
        }

        [Column(TypeName = "decimal(18,2)")]
        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }

        [Column(TypeName = "decimal(18,2)")]
        private decimal _tax;
        public decimal Tax
        {
            get => _tax;
            set => SetProperty(ref _tax, value);
        }

        [Column(TypeName = "decimal(18,2)")]
        private decimal _total;
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public Currency Currency { get; set; } = Currency.QAR;
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? IBAN { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        [MaxLength(500)]
        public string? DeliveryLocation { get; set; }

        // Make lines observable so UI can bind directly
        public ObservableCollection<PurchaseOrderLine> Lines { get; set; } = new ObservableCollection<PurchaseOrderLine>();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }
}