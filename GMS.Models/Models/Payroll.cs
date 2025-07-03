using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS.Models.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal TotalSalary { get; set; }
        public DateTime PayDate { get; set; }
        public Employee Employee { get; set; }
    }
}
