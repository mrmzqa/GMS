using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public class AccountProcess
        {
           public int Id { get; set; }
           public Payment Payment { get; set; }

            public AcccountPayable AccountPayable { get; set; }

            public AccountReceivable AccountReceivable { get; set; }


        }

        public class AcccountPayable
        {
            public int Id { get; set; }

        }

        public class AccountReceivable
        {
            public int Id { get; set; }
        }



    }
}
