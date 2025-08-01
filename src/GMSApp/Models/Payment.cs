using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public class PaymentReceipt
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string PayerName { get; set; }
            public PaymentMethod PaymentMethod { get; set; }
            public PaymentReceipt ReceiptNumber { get; set; }
            public int AdvancePayment { get; set; }
        }

        public class PaymentMethod
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }

            public string Remarks { get; set; }
          
        }

        public class ReceiptStatus
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }

            public string Remarks { get; set; }
        }

        public class ReceiptStatusUpdate
        {
            public int Id { get; set; }
            public ReceiptStatus Status { get; set; }
            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        }


        public class PaymentReceiptUpdate
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string PayerName { get; set; }
            public string PaymentMethod { get; set; }
            public string ReceiptNumber { get; set; }
            public int AdvancePayment { get; set; }
        }


        public class PaymentReceiptSearch
        {
            public string PayerName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? MinAmount { get; set; }
            public decimal? MaxAmount { get; set; }
            public PaymentMethod? Method { get; set; }
        }
        public class PaymentReceiptList
        {
            public List<Payment> Receipts { get; set; } = new List<Payment>();
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

    }
  










}