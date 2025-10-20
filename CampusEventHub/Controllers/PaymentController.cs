using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CampusEventHub.Service;
using System.Text.Json;
using CampusEventHub.Data;
using CampusEventHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampusEventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOSService _payOSService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PaymentController(PayOSService payOSService, IConfiguration configuration, ApplicationDbContext context)
        {
            _payOSService = payOSService;
            _configuration = configuration;
            _context = context;
        }
        
        [HttpPost("CreatePaymentUrl")]
        public async Task<ActionResult<string>> CreatePaymentUrl([FromBody] PaymentRequestModel model)
        {
            try
            {
                var random = new Random();
                var orderCode = random.Next(100000, 999999);
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Người dùng chưa đăng nhập (session không tồn tại)");
                }
        
                var baseUrl = _configuration["PayOS:BaseUrl"];
                var returnUrl = $"{baseUrl}/api/Payment/Return";
                var cancelUrl = $"{baseUrl}/api/Payment/Cancel";

                // Lưu thông tin đơn hàng
                var paymentOrder = new PaymentOrder
                {
                    OrderCode = orderCode,
                    UserId = userId,
                    Amount = (decimal)model.MoneyToPay,
                    Status = "PENDING",
                    CreatedAt = DateTime.Now
                };
                _context.PaymentOrders.Add(paymentOrder);
                await _context.SaveChangesAsync();

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
                        // Lấy thông tin người dùng (giả sử bạn có cách xác định UserId, ví dụ từ phiên đăng nhập)
                        var userId = User.Identity?.Name; // Hoặc lấy từ session, token, v.v.
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                        if (user == null)
                        {
                            return BadRequest(new { error = "Không tìm thấy người dùng." });
                        }

                        // Cập nhật Balance
                        user.Balance += paymentInfo.Amount; // Cộng số tiền thanh toán vào Balance
                        await _context.SaveChangesAsync();

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
        
        var signature = Request.Headers["x-payos-signature"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(signature))
        {
            return BadRequest(new { error = "Missing signature" });
        }

        var isValid = await _payOSService.VerifyWebhookAsync(body, signature);
        
        if (isValid)
        {
            var webhookData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(body);
            
            if (webhookData.TryGetProperty("data", out var data))
            {
                var orderCode = data.TryGetProperty("orderCode", out var oc) ? oc.GetInt32() : 0;
                var status = data.TryGetProperty("status", out var st) ? st.GetString() : "UNKNOWN";
                var amount = data.TryGetProperty("amount", out var amt) ? amt.GetInt32() : 0;

                var paymentOrder = await _context.PaymentOrders
                    .FirstOrDefaultAsync(po => po.OrderCode == orderCode);

                if (paymentOrder == null)
                {
                    return BadRequest(new { error = "Không tìm thấy đơn hàng." });
                }

                if (status == "PAID" && paymentOrder.Status != "PAID")
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == paymentOrder.UserId);

                    if (user != null)
                    {
                        user.Balance += paymentOrder.Amount;
                        paymentOrder.Status = "PAID";
                        await _context.SaveChangesAsync();
                    }
                }
                
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