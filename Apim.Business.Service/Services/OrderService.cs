using Apim.Business.Service.Interfaces;
using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Business.Service.Services
{
    public class OrderService : IOrderService
    {
        public IOrderRepository OrderRepository { get; set; }
        public OrderService(IOrderRepository orderRepository)
        {
            OrderRepository = orderRepository;
        }
        public List<Order> GetOrders()
        {
            return OrderRepository.Orders();
        }

        public Order GetOrder(int id)
        {
            return OrderRepository.GetOrder(id);
        }
    }
}
