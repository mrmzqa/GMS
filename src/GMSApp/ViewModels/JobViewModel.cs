using GMSApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GMSApp.Models.Job;


namespace GMSApp.ViewModels
{
    public class JobViewModel
    {
        public List<Jobcard> Jobcards { get; set; }

        public List<Quotation> Quotations { get; set; }

        public List<Models.PurchaseOrder> PurchaseOrders { get; set; }

        public List<Models.Inventory> Inventory { get; set; }

       


        
    }
}
