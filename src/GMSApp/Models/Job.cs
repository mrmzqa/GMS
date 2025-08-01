using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;



namespace GMSApp.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        public int Mainid { get; set; }

        public Main Main { get; set; }



        public class Quotation
        {
            [Key]
            public int Id { get; set; }
            public string CustomerName { get; set; }
            
            public DateTime QuotationDate { get; set; }
            public string Description { get; set; }
            public decimal EstimatedCost { get; set; }
            public decimal ActualCost { get; set; }
            public Status Status { get; set; }
            public DateTime? CompletionDate { get; set; }

            public Jobcard Jobcard { get; set; }
            public string Jobcardid { get; set;}

            public Payment Payment { get; set; }
            public string paymentid {get; set;}

        }
        public class Jobcard
        {
            public int Id { get; set; }

            public Vendor vendorid { get; set; }

            public string CustomerName { get; set; }

            public DateTime JobDate { get; set; }
            public string JobDescription { get; set; }
            public decimal EstimatedCost { get; set; }
            public decimal ActualCost { get; set; }
            public Status Status { get; set; }
            public DateTime? CompletionDate { get; set; }

            public Quotation Quotationid { get; set; }

            public Payment Payment { get; set; }

        }
        public class JobcardStatus
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public class PurchaseOrder
        {
            public int Id { get; set; }

            public List<Vendor> Vendor { get; set; }
            public DateTime OrderDate { get; set; }
            public List<Jobcard> Jobcards { get; set; }

            public List<Quotation> Quotations { get; set; }

            public decimal TotalAmount { get; set; }

            public Payment Payment { get; set; }

        }
       
    }
}
