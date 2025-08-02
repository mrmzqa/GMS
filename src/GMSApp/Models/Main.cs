using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class Main
    {
        
            [Key]
            public int Id { get; set; }

            [Required, ForeignKey(nameof(CoreMain))]
            public int CoreMainId { get; set; }
            public CoreMain CoreMain { get; set; } = null!;

            [Required, MaxLength(200)]
            public string Name { get; set; } = string.Empty;

            [ForeignKey(nameof(Type))]
            public int? TypeId { get; set; }
            public Type? Type { get; set; }

            [ForeignKey(nameof(Label))]
            public int? LabelId { get; set; }
            public Labels? Label { get; set; }

            public string? Description { get; set; }

            // Navigation collections for sub-entities  
            public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
            public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
            public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
            public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
            public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

            
        



        public class Labels
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

  

       

    }
}
