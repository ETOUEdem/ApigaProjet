using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.Models
{
   public class BillingDetail
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}
