using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class CoreMain
    {
        public int id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Main> Main { get; set; } 


    }
}
