# Hướng dẫn tích hợp payOS vào CampusEventHub

## Tổng quan
Dự án đã được tích hợp thành công với cổng thanh toán payOS để thay thế VnPay. payOS cung cấp giải pháp thanh toán Napas 24/7 với giao diện đẹp và dễ sử dụng.

## Cấu hình payOS

### 1. Tạo tài khoản payOS
- Truy cập [https://my.payos.vn](https://my.payos.vn) để tạo tài khoản
- Đăng ký và xác thực thông tin doanh nghiệp

### 2. Lấy thông tin API
Sau khi đăng ký thành công, bạn sẽ có:
- **Client ID**: ID ứng dụng của bạn
- **API Key**: Khóa API để xác thực
- **Checksum Key**: Khóa để xác thực webhook

### 3. Cập nhật cấu hình
Mở file `appsettings.json` và cập nhật thông tin payOS:

```json
{
  "PayOS": {
    "ClientId": "YOUR_PAYOS_CLIENT_ID",
    "ApiKey": "YOUR_PAYOS_API_KEY", 
    "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY",
    "BaseUrl": "https://your-domain.com"
  }
}
```

### 4. Cấu hình Webhook
Trong dashboard payOS, cấu hình webhook URL:
- **Webhook URL**: `https://your-domain.com/api/Payment/Webhook`
- **Return URL**: `https://your-domain.com/api/Payment/Return`
- **Cancel URL**: `https://your-domain.com/api/Payment/Cancel`

## Cấu trúc tích hợp

### 1. PayOSService
- **File**: `Service/PayOSService.cs`
- **Chức năng**: Xử lý các API calls với payOS
- **Methods**:
  - `CreatePaymentLinkAsync()`: Tạo link thanh toán
  - `VerifyWebhookAsync()`: Xác thực webhook
  - `GetPaymentInformationAsync()`: Lấy thông tin thanh toán
  - `CancelPaymentLinkAsync()`: Hủy link thanh toán

### 2. PaymentController
- **File**: `Controllers/PaymentController.cs`
- **Endpoints**:
  - `POST /api/Payment/CreatePaymentUrl`: Tạo link thanh toán
  - `GET /api/Payment/Return`: Xử lý kết quả thanh toán thành công
  - `GET /api/Payment/Cancel`: Xử lý hủy thanh toán
  - `POST /api/Payment/Webhook`: Nhận webhook từ payOS

### 3. Views
- **Payment/Index.cshtml**: Giao diện thanh toán với JavaScript
- **Payment/Success.cshtml**: Trang thành công
- **Payment/Cancel.cshtml**: Trang hủy thanh toán

## Luồng thanh toán

1. **Tạo thanh toán**: User click "Thanh toán với payOS"
2. **API Call**: Frontend gọi `/api/Payment/CreatePaymentUrl`
3. **Redirect**: Chuyển hướng đến trang thanh toán payOS
4. **Thanh toán**: User quét QR code hoặc chuyển khoản
5. **Return**: payOS redirect về `/api/Payment/Return`
6. **Webhook**: payOS gửi webhook đến `/api/Payment/Webhook`

## Testing

### 1. Sandbox Mode
- Sử dụng tài khoản sandbox để test
- Không cần thực sự chuyển tiền

### 2. Test Cards
payOS cung cấp các thẻ test để kiểm tra:
- Thẻ thành công: 4111111111111111
- Thẻ thất bại: 4000000000000002

## Lưu ý quan trọng

1. **Bảo mật**: Không commit API keys vào Git
2. **HTTPS**: Đảm bảo sử dụng HTTPS trong production
3. **Webhook**: Luôn xác thực webhook để đảm bảo tính toàn vẹn
4. **Error Handling**: Xử lý đầy đủ các trường hợp lỗi

## Troubleshooting

### Lỗi thường gặp:
1. **Invalid API Key**: Kiểm tra lại API Key trong appsettings.json
2. **Webhook không hoạt động**: Kiểm tra URL webhook và HTTPS
3. **Redirect không đúng**: Kiểm tra BaseUrl trong cấu hình

### Logs:
- Kiểm tra console logs để debug
- Webhook logs được ghi trong Console.WriteLine

## Tài liệu tham khảo
- [payOS Documentation](https://payos.vn/docs/)
- [payOS .NET SDK](https://github.com/payOSHQ/payos-demo-dotnet-core)
- [payOS Dashboard](https://my.payos.vn)
