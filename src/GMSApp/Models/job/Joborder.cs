using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.job
{
    public class Joborder
    {
        [Key]
        public int Id { get; set; }

        
        public string? CustomerName { get; set; }

       
        public string? Phonenumber { get; set; }

        
        public string? VehicleNumber { get; set; }

        
        public string? Brand { get; set; }

        
        public string? Model { get; set; }

       
        public Decimal? OdoNumber { get; set; }

        public ICollection<ItemRow> Items { get; set; } = new List<ItemRow>();

        public byte[]? F { get; set; }

        public string? FN { get; set; }

        public byte[]? B { get; set; }

        public string? BN { get; set; }

        public byte[]? LS { get; set; }

        public string? LSN { get; set; }
        public byte[]? RS { get; set; }

        public string? RSN { get; set; }

        public DateTime? Created { get; set; } = DateTime.Now;
         

    }
}
