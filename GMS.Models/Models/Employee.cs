using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS.Models.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }

        public string JoiningDate { get; set; }

        public string Department { get; set; }

        public string Email { get; set; }


        public string Passportnumber { get; set; }


        public string QID { get; set; }

        public ICollection<Attendance> Attendances { get; set; }
    }
}
