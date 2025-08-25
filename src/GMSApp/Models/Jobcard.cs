using GMSApp.Views.Job;

namespace GMSApp.Models
{
    public class Jobcard
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public DateTime JobDate { get; set; }

        public string JobDescription { get; set; }


        public decimal EstimatedCost { get; set; }


        public decimal ActualCost { get; set; }

        public Status Status { get; set; }

        public DateTime? CompletionDate { get; set; }
        public List<FileItem> Attachments { get; set; } = new List<FileItem>();

        public Quotation Quotation { get; set; }
        public string Quotationid { get; set; }
        public Payment Payment { get; set; }
        public string paymentid { get; set; }
       

    }
}