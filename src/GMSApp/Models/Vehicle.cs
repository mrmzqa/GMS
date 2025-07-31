using System.ComponentModel.DataAnnotations;

namespace GMSApp.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [MaxLength(17)]
        public string? VIN { get; set; }

        // Image stored as byte[] and its content type
        public byte[]? Image { get; set; }

        public string ImageContentType { get; set; } = string.Empty;
    }
}