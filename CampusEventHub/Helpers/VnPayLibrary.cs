using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CampusEventHub.Helpers
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> requestData = new SortedList<string, string>();
        private readonly SortedList<string, string> responseData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return responseData.ContainsKey(key) ? responseData[key] : string.Empty;
        }

        /// <summary>
        /// Tạo URL redirect sang VNPAY
        /// </summary>
        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var query = string.Join("&", requestData
                .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

            string signData = string.Join("&", requestData
                .Select(kvp => $"{kvp.Key}={kvp.Value}"));

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            return $"{baseUrl}?{query}&vnp_SecureHash={vnp_SecureHash}";
        }

        /// <summary>
        /// Kiểm tra chữ ký khi VNPAY trả về
        /// </summary>
        public bool ValidateSignature(string inputHash, string vnp_HashSecret)
        {
            var signData = string.Join("&", responseData
                .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                .Select(kvp => $"{kvp.Key}={kvp.Value}"));

            string myHash = HmacSHA512(vnp_HashSecret, signData);
            return myHash.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }
    }
}
