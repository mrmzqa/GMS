using System;
using System.ComponentModel.DataAnnotations;

namespace GarageApp.Models
{
    public class VehicleMakeModel
    {
        [Key]
        public Guid MakeModelId { get; set; } = Guid.NewGuid();

        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? EngineType { get; set; }
        public string? Transmission { get; set; }
        public int ServiceInterval { get; set; }  // in km or months
        public string? Notes { get; set; }
    }
}