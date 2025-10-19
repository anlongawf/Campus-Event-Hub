using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CampusEventHub.Service;
using System.Text.Json;

namespace CampusEventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOSService _payOSService;
        private readonly IConfiguration _configuration;

        public PaymentController(PayOSService payOSService, IConfiguration configuration)
        {
            _payOSService = payOSService;
            _configuration = configuration;
        }

        [HttpPost("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreatePaymentUrl([FromBody] PaymentRequestModel model)
        {
            try
            {
                // Tạo mã đơn hàng duy nhất
                var random = new Random();
                var orderCode = random.Next(100000, 999999);
                
                // Tạo URL trả về và hủy
                var baseUrl = _configuration["PayOS:BaseUrl"];
                var returnUrl = $"{baseUrl}/api/Payment/Return";
                var cancelUrl = $"{baseUrl}/api/Payment/Cancel";

                // Tạo link thanh toán payOS
                var paymentUrl = await _payOSService.CreatePaymentLinkAsync(
                    orderCode,
                    (int)model.MoneyToPay,
                    model.Description,
                    returnUrl,
                    cancelUrl
                );

                return Ok(new { paymentUrl = paymentUrl, orderCode = orderCode });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Lỗi tạo link thanh toán: {ex.Message}" });
            }
        }

        [HttpGet("Return")]
        public async Task<ActionResult<string>> Return()
        {
            try
            {
                var orderCode = Request.Query["orderCode"].FirstOrDefault();
                var status = Request.Query["status"].FirstOrDefault();

                if (string.IsNullOrEmpty(orderCode))
                {
                    return BadRequest(new { error = "Không tìm thấy mã đơn hàng." });
                }

                if (status == "PAID")
                {
                    // Lấy thông tin thanh toán để xác thực
                    var paymentInfo = await _payOSService.GetPaymentInformationAsync(int.Parse(orderCode));
                    
                    if (paymentInfo.Status == "PAID")
                    {
                        // Redirect đến trang thành công
                        return Redirect("/PaymentMvc/Success");
                    }
                }

                return Redirect("/PaymentMvc/Cancel");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Lỗi xử lý kết quả thanh toán: {ex.Message}" });
            }
        }

        [HttpGet("Cancel")]
        public ActionResult<string> Cancel()
        {
            return Redirect("/PaymentMvc/Cancel");
        }

        [HttpGet("Success")]
        public ActionResult<string> Success()
        {
            return Redirect("/PaymentMvc/Success");
        }

        [HttpPost("Webhook")]
        public async Task<ActionResult> Webhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                // Lấy signature từ header
                var signature = Request.Headers["x-payos-signature"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(signature))
                {
                    return BadRequest(new { error = "Missing signature" });
                }

                // Xác thực webhook
                var isValid = await _payOSService.VerifyWebhookAsync(body, signature);
                
                if (isValid)
                {
                    // Parse webhook data
                    var webhookData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(body);
                    
                    if (webhookData.TryGetProperty("data", out var data))
                    {
                        var orderCode = data.TryGetProperty("orderCode", out var oc) ? oc.GetInt32() : 0;
                        var status = data.TryGetProperty("status", out var st) ? st.GetString() : "UNKNOWN";
                        
                        // Xử lý logic cập nhật trạng thái đơn hàng ở đây
                        Console.WriteLine($"Webhook verified - Order: {orderCode}, Status: {status}");
                    }
                    
                    return Ok(new { message = "Webhook processed successfully" });
                }
                
                return BadRequest(new { error = "Invalid webhook signature" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook error: {ex.Message}");
                return BadRequest(new { error = $"Webhook error: {ex.Message}" });
            }
        }

        public class PaymentRequestModel
        {
            public double MoneyToPay { get; set; }
            public string Description { get; set; }
        }
    }
}