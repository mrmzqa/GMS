using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GMSApp.Models
{
    public class Vehicle
    {
        [Key]
        public Guid VehicleId { get; set; } = Guid.NewGuid();

        [Required]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        [MaxLength(50)]
        public string VIN { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServiceJob> ServiceJobs { get; set; } = new List<ServiceJob>();
    }
}