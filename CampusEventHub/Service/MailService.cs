using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace CampusEventHub.Service;

public class MailService
{
    private readonly string _fromEmail = "anan123456a123@gmail.com";
    private readonly string _password = "kycbfwaijbqyephd"; 
    
    bool ValidateEmail(string email)
    {
        string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        return Regex.IsMatch(email, pattern);
    }

    public string GenerateVerificationCode()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public void SendVerificationEmail(string recipientEmail, string code)
    {
        try
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("anan123456a123@gmail.com");
            mail.To.Add(recipientEmail);
            mail.Subject = "Email Verification";
            mail.Body = "Your verification code is: " + code;
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_fromEmail, _password);
            smtpClient.EnableSsl = true;
            smtpClient.Send(mail);

        }
        catch (Exception ex)
        {
            throw new Exception("Error sending verification email: " + ex.Message);
        }
    }
}