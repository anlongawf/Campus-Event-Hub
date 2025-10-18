using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CampusEventHub.Service
{
    public class PayOSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly string _baseUrl;
        private readonly bool _isSandbox;

        public PayOSService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _clientId = configuration["PayOS:ClientId"];
            _apiKey = configuration["PayOS:ApiKey"];
            _checksumKey = configuration["PayOS:ChecksumKey"];
            _baseUrl = configuration["PayOS:BaseUrl"]?.TrimEnd('/');
            _isSandbox = configuration.GetValue<bool>("PayOS:IsSandbox", true);

            _httpClient.BaseAddress = new Uri("https://api-merchant.payos.vn/v2/");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }

        public async Task<string> CreatePaymentLinkAsync(
            int orderCode,
            int amount,
            string description,
            string returnUrl,
            string cancelUrl)
        {
            try
            {
                // Tạo data theo format PayOS yêu cầu
                var dataToSign = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
                var signature = ComputeHmacSha256(dataToSign, _checksumKey);

                var requestBody = new
                {
                    orderCode = orderCode,
                    amount = amount,
                    description = description,
                    returnUrl = returnUrl,
                    cancelUrl = cancelUrl,
                    signature = signature,
                    items = new[]
                    {
                        new
                        {
                            name = description,
                            quantity = 1,
                            price = amount
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                Console.WriteLine($"Request Body: {jsonContent}");
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("payment-requests", content);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"PayOS Response: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"PayOS API Error ({response.StatusCode}): {responseString}");
                }

                var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                
                if (result.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("checkoutUrl", out var checkoutUrl))
                {
                    return checkoutUrl.GetString();
                }

                throw new Exception("Không thể lấy URL thanh toán từ PayOS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating payment: {ex.Message}");
                throw new Exception($"Lỗi tạo link thanh toán: {ex.Message}", ex);
            }
        }

        public async Task<PaymentInfo> GetPaymentInformationAsync(int orderCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"payment-requests/{orderCode}");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"PayOS API Error: {responseString}");
                }

                var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                
                if (result.TryGetProperty("data", out var data))
                {
                    return new PaymentInfo
                    {
                        OrderCode = data.TryGetProperty("orderCode", out var oc) ? oc.GetInt32() : 0,
                        Amount = data.TryGetProperty("amount", out var amt) ? amt.GetInt32() : 0,
                        Status = data.TryGetProperty("status", out var st) ? st.GetString() : "UNKNOWN",
                        Description = data.TryGetProperty("description", out var desc) ? desc.GetString() : ""
                    };
                }

                throw new Exception("Không thể lấy thông tin thanh toán");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy thông tin thanh toán: {ex.Message}", ex);
            }
        }

        public async Task<bool> VerifyWebhookAsync(string body, string signature)
        {
            try
            {
                var computedSignature = ComputeHmacSha256(body, _checksumKey);
                return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private string CreateSignature(int orderCode, int amount, string description, string returnUrl, string cancelUrl)
        {
            // Sắp xếp theo thứ tự alphabet của key
            var data = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            return ComputeHmacSha256(data, _checksumKey);
        }

        private string ComputeHmacSha256(string message, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }

    public class PaymentInfo
    {
        public int OrderCode { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}