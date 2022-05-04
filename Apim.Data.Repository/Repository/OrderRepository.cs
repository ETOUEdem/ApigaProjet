using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.Repository
{
    public class OrderRepository : IOrderRepository
    {
        public Order GetOrder(int key)
        {
            var l = Orders();
            return l.Find(x => x.Id == key);
        }

        public List<Order> Orders() {

            List<Order> orders = new List<Order>();
            var BillingDetailsFake = new Faker<BillingDetail>()
              .RuleFor(x => x.CustomerName, x => x.Person.FullName)
             .RuleFor(x => x.Email, x => x.Person.Email)
             .RuleFor(x => x.Phone, x => x.Person.Phone)
             .RuleFor(x => x.AddressLine, x => x.Address.StreetAddress())
             .RuleFor(x => x.City, x => x.Address.City())
             .RuleFor(x => x.Country, x => x.Address.Country())
             .RuleFor(x => x.PostCode, x => x.Address.ZipCode());
            int i = 0;
            var orderFaker = new Faker<Order>()
               .RuleFor(x => x.Id, i++)
                  .RuleFor(x => x.Currency, x => x.Finance.Currency().Code)
                     .RuleFor(x => x.Price, x => x.Finance.Amount(5, 100))
                     .RuleFor(x => x.BillingDetails, x => BillingDetailsFake);

            foreach (var order in orderFaker.Generate(100))
            {
                  order.Id = i++;
                  orders.Add(order);
            }
            return orders;


        }
    }
}
