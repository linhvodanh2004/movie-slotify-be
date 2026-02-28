using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["Smtp:Host"];
            var portString = _configuration["Smtp:Port"];
            var user = _configuration["Smtp:User"];
            var pass = _configuration["Smtp:Pass"];
            var from = _configuration["Smtp:From"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                // Optionally log info if SMTP is not configured
                throw new Exceptions.BadRequestException("SMTP is not configured properly.");
            }

            int port = 587;
            int.TryParse(portString, out port);

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var clientBaseUrl = _configuration["ClientBaseUrl"] ?? "http://localhost:3000";
            var resetLink = $"{clientBaseUrl.TrimEnd('/')}/reset-password?token={resetToken}";
            
            var emailBody = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Đặt lại mật khẩu</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f6f8;
            margin: 0;
            padding: 0;
            color: #333333;
        }}
        .email-container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #FF6B6B, #FF8E53);
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            color: #ffffff;
            margin: 0;
            font-size: 24px;
            font-weight: 600;
            letter-spacing: 1px;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 25px;
            color: #555555;
        }}
        .btn-container {{
            text-align: center;
            margin: 35px 0;
        }}
        .btn {{
            display: inline-block;
            background-color: #FF6B6B;
            color: #ffffff !important;
            text-decoration: none;
            padding: 14px 35px;
            font-size: 16px;
            font-weight: bold;
            border-radius: 50px;
            box-shadow: 0 4px 10px rgba(255, 107, 107, 0.3);
            transition: background-color 0.3s, transform 0.2s;
        }}
        .btn:hover {{
            background-color: #FF5252;
            transform: translateY(-2px);
        }}
        .footer {{
            background-color: #fcfcfc;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #eeeeee;
        }}
        .footer p {{
            font-size: 13px;
            color: #999999;
            margin: 0;
            line-height: 1.5;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1>MovieSlotify</h1>
        </div>
        <div class=""content"">
            <p>Xin chào,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại <b>MovieSlotify</b>.</p>
            <p>Vui lòng nhấn vào nút bên dưới để thiết lập mật khẩu mới. Đường dẫn này sẽ hết hạn trong vòng <b>15 phút</b>.</p>
            
            <div class=""btn-container"">
                <a href=""{resetLink}"" class=""btn"">Thiết lập mật khẩu mới</a>
            </div>
            
            <p>Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn và không có thay đổi nào được thực hiện.</p>
            <p>Trân trọng,<br>Đội ngũ MovieSlotify</p>
        </div>
        <div class=""footer"">
            <p>&copy; {System.DateTime.UtcNow.Year} MovieSlotify. Mọi bản quyền được bảo lưu.</p>
            <p>Bạn nhận được email này vì đã đăng ký tại MovieSlotify.</p>
        </div>
    </div>
</body>
</html>";
            await SendEmailAsync(toEmail, "MovieSlotify: Đặt lại mật khẩu", emailBody);
        }
    }
}
