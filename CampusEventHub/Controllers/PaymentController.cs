using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CampusEventHub.Models;
using CampusEventHub.Helpers;
namespace CampusEventHub.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;

        public PaymentController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Ví dụ tạo đơn hàng
            var order = new OrderInfo
            {
                OrderId = DateTime.Now.Ticks, // giả lập
                Amount = 100000,
                OrderDesc = "Thanh toán sự kiện A"
            };

            return View(order);
        }

        [HttpPost]
        public IActionResult CreatePayment(OrderInfo order)
        {
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", order.OrderDesc);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:ReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);
            return Redirect(paymentUrl);
        }

        // Xử lý khi VNPAY redirect về ReturnUrl
        [HttpGet]
        public IActionResult Return()
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var vnp_SecureHash = Request.Query["vnp_SecureHash"];
            var check = vnpay.ValidateSignature(vnp_SecureHash, _config["VnPay:HashSecret"]);

            if (check)
            {
                var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
                ViewBag.Message = responseCode == "00" ? "✅ Thanh toán thành công!" : $"❌ Thất bại (Mã lỗi: {responseCode})";
            }
            else
            {
                ViewBag.Message = "⚠️ Sai chữ ký (Invalid signature)";
            }

            return View();
        }
    }
}
