using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CampusEventHub.Helpers;
using CampusEventHub.Services;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace CampusEventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;

        public PaymentController(IVnpay vnpay, IConfiguration configuration)
        {
            _vnpay = vnpay;
            _configuration = configuration;

            _vnpay.Initialize(
                _configuration["VnPay:TmnCode"],
                _configuration["VnPay:HashSecret"],
                _configuration["VnPay:BaseUrl"],
                _configuration["VnPay:ReturnUrl"]
            );
        }

        [HttpPost("CreatePaymentUrl")]
        public ActionResult<string> CreatePaymentUrl([FromBody] PaymentRequestModel model)
        {
            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = model.MoneyToPay,
                Description = model.Description,
                IpAddress = NetworkHelper.GetIpAddress(HttpContext),
                BankCode = BankCode.ANY,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };

            var paymentUrl = _vnpay.GetPaymentUrl(request);
            return Created(paymentUrl, paymentUrl);
        }

        [HttpGet("Callback")]
        public ActionResult<string> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var resultDescription = $"{paymentResult.PaymentResponse.Description}. {paymentResult.TransactionStatus.Description}.";

                    // In ra console để test
                    Console.WriteLine(resultDescription);

                    if (paymentResult.IsSuccess)
                        return Ok($"Thanh toán thành công: {resultDescription}");

                    return BadRequest($"Thanh toán thất bại: {resultDescription}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Lỗi: {ex.Message}");
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        public class PaymentRequestModel
        {
            public double MoneyToPay { get; set; }
            public string Description { get; set; }
        }
    }
}
