using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GMSApp.Models
{
    

    public class FileItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public byte[] Data { get; set; } = [];

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

}
