using Apim.Data.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.Interfaces
{
   public interface IOrderRepository
    {
        List<Order> Orders();
        Order GetOrder(int key);

    }
}
