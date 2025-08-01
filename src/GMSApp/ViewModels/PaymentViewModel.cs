using GMSApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public class PaymentViewModel
    {
        public class PaymentReceiptViewModel
        {
            public List<Payment.PaymentReceipt> Receipts { get; set; } = new List<Payment.PaymentReceipt>();
            public PaymentReceiptViewModel()
            {
                Receipts = new List<Payment.PaymentReceipt>();
            }
            public void AddReceipt(Payment.PaymentReceipt receipt)
            {
                Receipts.Add(receipt);
            }
            public void RemoveReceipt(int id)
            {
                var receipt = Receipts.FirstOrDefault(r => r.Id == id);
                if (receipt != null)
                {
                    Receipts.Remove(receipt);
                }
            }
            public void UpdateReceipt(Payment.PaymentReceipt updatedReceipt)
            {
                var receipt = Receipts.FirstOrDefault(r => r.Id == updatedReceipt.Id);
                if (receipt != null)
                {
                    receipt.Amount = updatedReceipt.Amount;
                    receipt.PaymentDate = updatedReceipt.PaymentDate;
                    receipt.PayerName = updatedReceipt.PayerName;
                    receipt.PaymentMethod = updatedReceipt.PaymentMethod;
                    receipt.ReceiptNumber = updatedReceipt.ReceiptNumber;
                    receipt.AdvancePayment = updatedReceipt.AdvancePayment;
                }
            }
        }

        public class PaymentReceiptSearchViewModel
        {
            public List<Payment> SearchResults { get; set; } = new List<Payment>();
            public PaymentReceiptSearchViewModel()
            {
                SearchResults = new List<Payment>();
            }
            public void AddSearchResult(Payment receipt)
            {
                SearchResults.Add(receipt);
            }
            public void ClearSearchResults()
            {
                SearchResults.Clear();
            }
        }


        public class PaymentReceiptFilter
        {
            public string? PayerName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? MinAmount { get; set; }
            public decimal? MaxAmount { get; set; }
            public Payment.PaymentMethod PaymentMethod { get; set; }
        }
    }
}
