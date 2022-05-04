using Apim.Data.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Business.Service.Interfaces
{
   public interface IOrderService
    {
        List<Order> GetOrders();
        Order GetOrder(int id);
    }
}
