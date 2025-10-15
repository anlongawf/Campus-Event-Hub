using Microsoft.Extensions.Configuration;
using VNPAY.NET;
using VNPAY.NET.Models;

namespace CampusEventHub.Services
{
    using VNPAY.NET;

    public class VnpayPayment
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;

        public VnpayPayment(IVnpay vnpay, IConfiguration configuration)
        {
            _vnpay = vnpay;
            _configuration = configuration;

            _vnpay.Initialize(_configuration["VnPay:TmnCode"], _configuration["VnPay:HashSecret"], _configuration["VnPay:BaseUrl"], _configuration["VnPay:CallbackUrl"]);
        }
    }
}