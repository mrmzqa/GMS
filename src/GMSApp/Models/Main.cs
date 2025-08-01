using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class Main
    {
        [Key]
        public int Id { get; set; }

        public int CoreMainId { get; set; }

        public CoreMain CoreMain { get; set; }
        public string Name { get; set; }

        public Type Type { get; set; }

        public labels label { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Inventory> Inventory { get; set; }

        public virtual ICollection<Account> account { get; set; }

        public virtual ICollection<Vendor> Vendor { get; set; }

        public virtual ICollection<Payment> Payment { get; set; }

        public virtual ICollection<Job> Job { get; set; }

        public class labels
        {
            [Key]
            public int Id { get; set; }
            public quotationlabel quotationlabels { get; set; }
            public inventorylabel inventorylabels { get; set; }
            public productlabel Productlabels { get; set; }
            public vendorlabel  vendorlabels { get; set; }
            public invoicelabel invoicelabels { get; set; }

        }

        public class quotationlabel
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public string Field { get; set; }


        }
        public class inventorylabel
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public string Field { get; set; }
        }

        public class productlabel
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public string Field { get; set; }

        }
        public class vendorlabel
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public string Field { get; set; }
        }
        public class invoicelabel
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }

            public string Field { get; set; }
        }

        public byte[] headerfile { get; set; }

        public string headername { get; set; }

        public byte[] footerfile { get; set; }

        public string footername { get; set; }

       

    }
}
