using QRCoder;
using System;

public static class QRCodeHelper
{
    public static string GenerateQRCodeBase64(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        
        using var qrCode = new SvgQRCode(qrCodeData);
        string svgText = qrCode.GetGraphic(10);
        
        string base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svgText));
        return $"data:image/svg+xml;base64,{base64}";
    }
}