using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMSApp.Models
{
   
    public class CoreMain
    {
        [Key]
        public int Id { get; set; }

       
        public string? Name { get; set; } 

        public byte[]? HeaderFile { get; set; }
        public string? HeaderName { get; set; }

        public byte[]? FooterFile { get; set; }
        public string? FooterName { get; set; }

        public virtual ICollection<Main> Main { get; set; } = new List<Main>();
    }

}