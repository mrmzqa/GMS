using System.ComponentModel.DataAnnotations;

namespace GMSApp.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int MainId { get; set; }

        public Main Main { get; set; }
        public List<VendorData>Vendors { get; set; }
        public class VendorData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ContactPerson { get; set; }
            public Address Address { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        public class Address
        {
            [Key]
            public int Id { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Country { get; set; }
            public string email { get; set; }
            public int phone { get; set; }

        }
    }
}

