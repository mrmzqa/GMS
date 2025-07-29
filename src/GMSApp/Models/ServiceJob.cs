using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApp.Models
{
    public class ServiceJob
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        [Required]
        public string ReportedIssue { get; set; } = string.Empty;

        public string? Diagnosis { get; set; }
        public string? FixesPerformed { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public Guid? AssignedWorkerId { get; set; }

        [ForeignKey(nameof(AssignedWorkerId))]
        public GarageWorker? AssignedWorker { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Cost { get; set; }
    }
}