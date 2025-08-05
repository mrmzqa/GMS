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

       


        public class JobcardViewModel
        {
            public List<Jobcard> Jobcards { get; set; } = new List<Jobcard>();
            public void AddJobcard(Jobcard jobcard)
            {
                Jobcards.Add(jobcard);
            }
            public void RemoveJobcard(int id)
            {
                var jobcard = Jobcards.FirstOrDefault(j => j.Id == id);
                if (jobcard != null)
                {
                    Jobcards.Remove(jobcard);
                }
            }
            public void UpdateJobcard(Jobcard updatedJobcard)
            {
                var jobcard = Jobcards.FirstOrDefault(j => j.Id == updatedJobcard.Id);
                if (jobcard != null)
                {
                    jobcard.CustomerName = updatedJobcard.CustomerName;
                    jobcard.JobDate = updatedJobcard.JobDate;
                    jobcard.JobDescription = updatedJobcard.JobDescription;
                    jobcard.EstimatedCost = updatedJobcard.EstimatedCost;
                    jobcard.ActualCost = updatedJobcard.ActualCost;
                    jobcard.Status = updatedJobcard.Status;
                    jobcard.CompletionDate = updatedJobcard.CompletionDate;
                }
            }
        }
    }
}
