using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.Models
{
 
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // "Pending", "Completed", "Cancelled"
        
        public int? UserId { get; set; }
        public User User { get; set; }
        
        public ICollection<OrderItem> OrderItems { get; set; }
    }

}
