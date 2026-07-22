// Talabat.Services/EmailService/SendGridEmailService.cs
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Talabat.Core.Service;

namespace Talabat.Services.EmailService
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(IConfiguration configuration)
        {
            _apiKey = configuration["SendGridSettings:ApiKey"]!;
            _fromEmail = configuration["SendGridSettings:FromEmail"]!;
            _fromName = configuration["SendGridSettings:FromName"]!;
        }

        public async Task SendPaymentLinkAsync(string toEmail, string paymentUrl)
        {
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);

            var subject = "🍕 Talabat - رابط الدفع الخاص بك";

            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #ff6b6b;'>مرحباً بك في Talabat!</h2>
                    
                    <p>تم إنشاء طلبك بنجاح. اضغط على الزر أدناه لإتمام الدفع:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{paymentUrl}' 
                           style='background: #ff6b6b; color: white; padding: 15px 30px; 
                                  text-decoration: none; border-radius: 5px; display: inline-block;'>
                            💳 ادفع الآن
                        </a>
                    </div>
                    
                    <p style='color: #666; font-size: 14px;'>
                        أو انسخ هذا الرابط ولصقه في المتصفح:<br>
                        <a href='{paymentUrl}'>{paymentUrl}</a>
                    </p>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    
                    <p style='color: #999; font-size: 12px;'>
                        ⚠️ هذا الرابط صالح لمدة 24 ساعة فقط.<br>
                        إذا لم تقم بالدفع خلال هذا الوقت، سيتم إلغاء طلبك تلقائياً.
                    </p>
                </div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email: {error}");
            }
        }
    }
}