using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS.Models.Models
{
    public class JobOrder
    {
        public int Id { get; set; }
        public int QuotationId { get; set; }
        public string VehicleDetails { get; set; }
        public DateTime JobDate { get; set; }
        public Quotation Quotation { get; set; }
    }
}
