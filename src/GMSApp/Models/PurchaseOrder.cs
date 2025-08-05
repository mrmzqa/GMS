using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public List<ItemRow> Items { get; set; } = new();

        public decimal GrandTotal => Items?.Sum(i => i.Total) ?? 0;
    }
}
