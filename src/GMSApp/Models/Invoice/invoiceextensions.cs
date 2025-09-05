
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models.invoice
{
    public static class Invoiceextensions
    {
        public static void RecalculateTotals(this Invoice invoice)
        {
            if (invoice == null) return;

            decimal subtotal = 0m;
            foreach (var line in invoice.Lines)
            {
                line.LineTotal = Math.Round(line.UnitPrice * line.Quantity, 2);
                subtotal += line.LineTotal;
            }

            invoice.SubTotal = Math.Round(subtotal, 2);
            invoice.Total = Math.Round(invoice.SubTotal - invoice.Discount + invoice.Tax, 2);

            if (invoice.AmountPaid == 0)
                invoice.Status = InvoiceStatus.Pending;
            else if (invoice.AmountPaid < invoice.Total)
                invoice.Status = InvoiceStatus.PartiallyPaid;
            else if (invoice.AmountPaid >= invoice.Total)
                invoice.Status = InvoiceStatus.Paid;
        }
    }
}
