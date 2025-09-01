// File: Models/PurchaseOrderLine.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models
{
    // Line model implements INotifyPropertyChanged via ObservableObject so DataGrid edits update totals automatically.
    public class PurchaseOrderLine : ObservableObject
    {
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }

        [Required, MaxLength(250)]
        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        [MaxLength(100)]
        private string? _partNumber;
        public string? PartNumber
        {
            get => _partNumber;
            set => SetProperty(ref _partNumber, value);
        }

        private decimal _unitPrice;
        [Required]
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        private decimal _quantity;
        [Required]
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        [Column(TypeName = "decimal(18,2)")]
        private decimal _lineTotal;
        public decimal LineTotal
        {
            get => Math.Round(UnitPrice * Quantity, 2);
            // keep setter if EF needs to materialize; but primary calculation is derived
            set => SetProperty(ref _lineTotal, value);
        }

        [MaxLength(50)]
        private string? _unit = "pc";
        public string? Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        [MaxLength(250)]
        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private decimal _quantityDelivered;
        public decimal QuantityDelivered
        {
            get => _quantityDelivered;
            set => SetProperty(ref _quantityDelivered, value);
        }

        [NotMapped]
        public decimal QuantityPending => Quantity - QuantityDelivered;
    }
}