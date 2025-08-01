using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models
{
    public class Main
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        
        [ForeignKey(nameof(CoreMain))]
        public int CoreMainId { get; set; }

        public CoreMain CoreMain { get; set; }
    }
}