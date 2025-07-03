using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS.Models.Models
{
    public class VehicleDelivery
    {
        public int Id { get; set; }
        public int JobOrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public JobOrder JobOrder { get; set; }
    }
}
