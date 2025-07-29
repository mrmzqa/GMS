using System;
using System.ComponentModel.DataAnnotations;

namespace GarageApp.Models
{
    public class GarageWorker
    {
        [Key]
        public Guid WorkerId { get; set; } = Guid.NewGuid();

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Email { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}