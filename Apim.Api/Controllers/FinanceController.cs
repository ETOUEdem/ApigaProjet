using Apim.Business.Service.Interfaces;
using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apim.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        public IOrderService orderService { get; set; }
        public FinanceController(IOrderService OrderService)
        {
            orderService = OrderService;
        }

       
        [HttpGet("{GetOrders}")]
        public List<Order> GetOrders() {
            return orderService.GetOrders();
        }

        [HttpGet("{GetOrder}/{id}")]
        public Order GetOrder(int id)
        {
            return orderService.GetOrder(id);
        }
    }
}
