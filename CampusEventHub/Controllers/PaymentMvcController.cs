using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CampusEventHub.Service;

namespace CampusEventHub.Controllers
{
    public class PaymentMvcController : Controller
    {
        private readonly PayOSService _payOSService;
        private readonly IConfiguration _configuration;

        public PaymentMvcController(PayOSService payOSService, IConfiguration configuration)
        {
            _payOSService = payOSService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Tạo model OrderInfo mẫu để test
            var orderInfo = new Models.OrderInfo
            {
                OrderId = DateTime.Now.Ticks,
                Amount = 100000, // 100,000 VND
                OrderDesc = "Thanh toán vé sự kiện Campus Event Hub"
            };

            return View(orderInfo);
        }

        [HttpGet]
        public IActionResult TopUp()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Demo()
        {
            return View();
        }
    }
}
