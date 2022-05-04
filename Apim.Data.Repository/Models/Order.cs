using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.Models
{
    public class Order
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public BillingDetail BillingDetails { get; set; }
    }
}
