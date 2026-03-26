using BusinessLogic.DTOs.Notifications;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
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
                throw new Exceptions.BadRequestException("SMTP is not configured properly.");
            }

            int port = 587;
            int.TryParse(portString, out port);
            var fromAddress = ResolveFromAddress(user, from);

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public string GetCurrentSmtpUser()
        {
            return _configuration["Smtp:User"] ?? string.Empty;
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
            <p>Xin chao,</p>
            <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại <b>MovieSlotify</b>.</p>
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

            await SendEmailAsync(toEmail, "MovieSlotify - Đặt lại mật khẩu", emailBody);
        }

        public async Task SendBookingConfirmationEmailAsync(BookingConfirmationEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                throw new Exceptions.BadRequestException("Booking confirmation email is missing recipient address.");
            }

            var clientBaseUrl = (_configuration["ClientBaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
            var manageBookingUrl = $"{clientBaseUrl}/my-bookings";
            var subject = $"MovieSlotify - Xác nhận đặt vé #{request.BookingCode}";
            var emailBody = BuildBookingConfirmationEmailBody(request, manageBookingUrl);

            await SendEmailAsync(request.RecipientEmail, subject, emailBody);
        }

        private string BuildBookingConfirmationEmailBody(
            BookingConfirmationEmailRequest request,
            string manageBookingUrl)
        {
            var customerName = Encode(Fallback(request.RecipientName, request.RecipientEmail));
            var movieTitle = Encode(request.MovieTitle);
            var cinemaName = Encode(request.CinemaName);
            var auditoriumName = Encode(request.AuditoriumName);
            var cinemaAddress = Encode(Fallback(request.CinemaAddress, "Cap nhat tai muc Ve cua toi"));
            var paymentMethod = Encode(Fallback(request.PaymentMethod, "Online banking"));
            var transactionId = Encode(Fallback(request.TransactionId, "Dang cap nhat"));
            var bookingCode = Encode(request.BookingCode);
            var bookingId = Encode(request.BookingId);
            var genre = Encode(Fallback(request.MovieGenre, "Dang cap nhat"));
            var duration = request.DurationMinutes > 0 ? $"{request.DurationMinutes} phut" : "Dang cap nhat";
            var showDate = Encode(request.StartTime.ToString("dddd, dd/MM/yyyy", VietnameseCulture));
            var showTime = Encode($"{request.StartTime:HH:mm} - {request.EndTime:HH:mm}");
            var totalAmount = Encode(FormatCurrency(request.TotalAmount));
            var seatSummary = Encode(string.Join(", ", request.Tickets.Select(t => t.SeatLabel)));
            var ticketCount = request.Tickets.Count;
            var ticketRows = BuildTicketRows(request.Tickets);
            var seatBadges = BuildSeatBadges(request.Tickets);
            var posterBlock = BuildPosterBlock(request.MoviePosterUrl, request.MovieTitle);

            return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Xác nhận đặt vé</title>
</head>
<body style="margin:0;padding:0;background-color:#f4f1ea;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;color:#1f2937;">
    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#f4f1ea;margin:0;padding:24px 0;">
        <tr>
            <td align="center">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:680px;background-color:#ffffff;border-radius:28px;overflow:hidden;box-shadow:0 16px 48px rgba(15,23,42,0.10);">
                    <tr>
                        <td style="padding:0;background:linear-gradient(135deg,#111827 0%,#7c2d12 55%,#fb923c 100%);">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                <tr>
                                    <td style="padding:36px 36px 28px 36px;">
                                        <div style="font-size:12px;letter-spacing:0.24em;text-transform:uppercase;color:#fde68a;font-weight:700;margin-bottom:14px;">MovieSlotify</div>
                                        <div style="display:inline-block;padding:8px 14px;border-radius:999px;background-color:rgba(255,255,255,0.14);font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:#fff7ed;font-weight:700;margin-bottom:18px;">
                                            Thanh toán đã được xác nhận
                                        </div>
                                        <h1 style="margin:0 0 12px 0;font-size:34px;line-height:1.15;color:#ffffff;font-weight:800;">Ve xem phim cua ban da san sang</h1>
                                        <p style="margin:0;max-width:470px;font-size:16px;line-height:1.7;color:#ffedd5;">
                                            Xin chao {{customerName}}, chung toi da ghi nhan thanh cong don ve cua ban. Ban co the dua email nay khi den rap hoac mo muc Ve cua toi de xem lai thong tin bat cu luc nao.
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:0 36px 36px 36px;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:rgba(255,255,255,0.12);border:1px solid rgba(255,255,255,0.16);border-radius:22px;">
                                            <tr>
                                                <td style="padding:22px 24px;">
                                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                                        <tr>
                                                            <td style="padding-bottom:8px;font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:#fdba74;font-weight:700;">Ma don hang</td>
                                                            <td align="right" style="padding-bottom:8px;font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:#fdba74;font-weight:700;">Tong thanh toan</td>
                                                        </tr>
                                                        <tr>
                                                            <td style="font-size:26px;font-weight:800;color:#ffffff;">#{{bookingCode}}</td>
                                                            <td align="right" style="font-size:26px;font-weight:800;color:#ffffff;">{{totalAmount}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td colspan="2" style="padding-top:14px;font-size:13px;color:#ffedd5;line-height:1.6;">
                                                                {{ticketCount}} ve | Ghe {{seatSummary}} | Phuong thuc thanh toan: {{paymentMethod}}
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:32px 36px 12px 36px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:separate;border-spacing:0;">
                                <tr>
                                    <td valign="top" style="padding:0 20px 20px 0;">
                                        {{posterBlock}}
                                    </td>
                                    <td valign="top" style="padding:0 0 20px 0;">
                                        <div style="font-size:13px;letter-spacing:0.08em;text-transform:uppercase;color:#9a3412;font-weight:700;margin-bottom:10px;">Thông tin phim</div>
                                        <h2 style="margin:0 0 10px 0;font-size:28px;line-height:1.2;color:#111827;font-weight:800;">{{movieTitle}}</h2>
                                        <p style="margin:0 0 18px 0;font-size:15px;line-height:1.7;color:#4b5563;">
                                            {{genre}} | {{duration}}
                                        </p>
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#fff7ed;border:1px solid #fed7aa;border-radius:20px;">
                                            <tr>
                                                <td style="padding:18px 20px;">
                                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                                        <tr>
                                                            <td style="padding:0 0 14px 0;font-size:13px;color:#9a3412;font-weight:700;">Ngày chiếu</td>
                                                            <td align="right" style="padding:0 0 14px 0;font-size:15px;color:#111827;font-weight:700;">{{showDate}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td style="padding:0 0 14px 0;font-size:13px;color:#9a3412;font-weight:700;">Khung giờ</td>
                                                            <td align="right" style="padding:0 0 14px 0;font-size:15px;color:#111827;font-weight:700;">{{showTime}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td style="padding:0 0 14px 0;font-size:13px;color:#9a3412;font-weight:700;">Rạp / Phòng</td>
                                                            <td align="right" style="padding:0 0 14px 0;font-size:15px;color:#111827;font-weight:700;">{{cinemaName}} / {{auditoriumName}}</td>
                                                        </tr>
                                                        <tr>
                                                            <td style="padding:0;font-size:13px;color:#9a3412;font-weight:700;">Địa chỉ</td>
                                                            <td align="right" style="padding:0;font-size:15px;color:#111827;line-height:1.6;">{{cinemaAddress}}</td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:0 36px 8px 36px;">
                            <div style="font-size:13px;letter-spacing:0.08em;text-transform:uppercase;color:#9a3412;font-weight:700;margin-bottom:12px;">Thông tin ghế</div>
                            <div style="margin-bottom:18px;">{{seatBadges}}</div>
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #e5e7eb;border-radius:20px;overflow:hidden;">
                                <tr>
                                    <td style="padding:0;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                            <tr style="background-color:#111827;">
                                                <td style="padding:14px 18px;font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:#e5e7eb;font-weight:700;">Ghe</td>
                                                <td style="padding:14px 18px;font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:#e5e7eb;font-weight:700;">Loai</td>
                                                <td align="right" style="padding:14px 18px;font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:#e5e7eb;font-weight:700;">Gia</td>
                                            </tr>
                                            {{ticketRows}}
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:24px 36px 0 36px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#f8fafc;border:1px solid #e2e8f0;border-radius:20px;">
                                <tr>
                                    <td style="padding:22px 24px;">
                                        <div style="font-size:13px;letter-spacing:0.08em;text-transform:uppercase;color:#334155;font-weight:700;margin-bottom:14px;">Thanh toán và đối soát</div>
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                            <tr>
                                                <td style="padding:0 0 12px 0;font-size:14px;color:#475569;font-weight:600;">Ma booking</td>
                                                <td align="right" style="padding:0 0 12px 0;font-size:15px;color:#0f172a;font-weight:700;">{{bookingId}}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding:0 0 12px 0;font-size:14px;color:#475569;font-weight:600;">Ma giao dich</td>
                                                <td align="right" style="padding:0 0 12px 0;font-size:15px;color:#0f172a;font-weight:700;">{{transactionId}}</td>
                                            </tr>
                                            <tr>
                                                <td style="padding:0;font-size:14px;color:#475569;font-weight:600;">Tong tien</td>
                                                <td align="right" style="padding:0;font-size:18px;color:#ea580c;font-weight:800;">{{totalAmount}}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:28px 36px 0 36px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:linear-gradient(135deg,#111827 0%,#1f2937 100%);border-radius:22px;">
                                <tr>
                                    <td style="padding:26px 28px;">
                                        <h3 style="margin:0 0 10px 0;font-size:20px;color:#ffffff;font-weight:800;">Sẵn sàng cho suất chiếu?</h3>
                                        <p style="margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#d1d5db;">
                                            Bạn nên có mặt tại rạp trước ít nhất 15 phút để đối soát và vào phòng chiếu đúng giờ. Nếu cần xem lại vé, lịch sử đặt vé đã được cập nhật ngay trong tài khoản của bạn.
                                        </p>
                                        <a href="{{manageBookingUrl}}" style="display:inline-block;padding:14px 24px;border-radius:999px;background-color:#f97316;color:#fff7ed;text-decoration:none;font-size:14px;font-weight:800;letter-spacing:0.02em;">
                                            Xem ve cua toi
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:24px 36px 36px 36px;">
                            <p style="margin:0;font-size:13px;line-height:1.7;color:#6b7280;">
                                Email này được gửi tự động sau khi hệ thống ghi nhận thanh toán thành công. Nếu bạn cần hỗ trợ, vui lòng liên hệ bộ phận chăm sóc khách hàng của MovieSlotify và cung cấp mã đơn <strong>#{{bookingCode}}</strong>.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:18px 36px;background-color:#fffaf5;border-top:1px solid #fed7aa;text-align:center;">
                            <p style="margin:0;font-size:12px;line-height:1.6;color:#9a3412;">
                                &copy; {{System.DateTime.UtcNow.Year}} MovieSlotify. Cảm ơn bạn đã đặt vé tại hệ thống của chúng tôi.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
""";
        }

        private static string BuildTicketRows(IEnumerable<BookingConfirmationTicketItem> tickets)
        {
            var sb = new StringBuilder();
            var index = 0;

            foreach (var ticket in tickets)
            {
                var backgroundColor = index % 2 == 0 ? "#ffffff" : "#fffaf5";
                sb.Append(
                    $$"""
                                            <tr style="background-color:{{backgroundColor}};">
                                                <td style="padding:16px 18px;font-size:15px;color:#111827;font-weight:700;border-top:1px solid #e5e7eb;">{{Encode(ticket.SeatLabel)}}</td>
                                                <td style="padding:16px 18px;font-size:14px;color:#4b5563;border-top:1px solid #e5e7eb;">{{Encode(ticket.SeatType)}}</td>
                                                <td align="right" style="padding:16px 18px;font-size:14px;color:#111827;font-weight:700;border-top:1px solid #e5e7eb;">{{Encode(FormatCurrency(ticket.Price))}}</td>
                                            </tr>
"""
                );
                index++;
            }

            return sb.ToString();
        }

        private static string BuildSeatBadges(IEnumerable<BookingConfirmationTicketItem> tickets)
        {
            return string.Join(
                string.Empty,
                tickets.Select(
                    ticket =>
                        $$"""
<span style="display:inline-block;margin:0 10px 10px 0;padding:10px 14px;border-radius:999px;background-color:#fff7ed;border:1px solid #fdba74;font-size:13px;color:#9a3412;font-weight:800;">
    {{Encode(ticket.SeatLabel)}} <span style="font-weight:600;color:#c2410c;">{{Encode(ticket.SeatType)}}</span>
</span>
"""
                )
            );
        }

        private static string BuildPosterBlock(string? posterUrl, string movieTitle)
        {
            if (string.IsNullOrWhiteSpace(posterUrl))
            {
                return
                    """
<div style="width:180px;height:256px;border-radius:24px;background:linear-gradient(180deg,#111827 0%,#374151 100%);padding:22px;box-sizing:border-box;">
    <div style="font-size:12px;letter-spacing:0.18em;text-transform:uppercase;color:#fbbf24;font-weight:700;margin-bottom:18px;">Now Showing</div>
    <div style="font-size:28px;line-height:1.25;color:#ffffff;font-weight:800;">Movie night</div>
    <div style="margin-top:18px;font-size:14px;line-height:1.7;color:#d1d5db;">Thông tin chiếu phim và vé đã được xác nhận trong email này.</div>
</div>
""";
            }

            return $$"""
<img src="{{Encode(posterUrl)}}" alt="{{Encode(movieTitle)}}" width="180" style="display:block;width:180px;max-width:180px;height:256px;object-fit:cover;border-radius:24px;border:1px solid #fed7aa;" />
""";
        }

        private static string FormatCurrency(decimal amount)
        {
            return $"{amount.ToString("#,0", VietnameseCulture)} VND";
        }

        private static string Fallback(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string Encode(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string ResolveFromAddress(string user, string? configuredFrom)
        {
            if (string.IsNullOrWhiteSpace(configuredFrom)) return user;
            if (string.IsNullOrWhiteSpace(user)) return configuredFrom;
            if (!MailAddress.TryCreate(configuredFrom, out _)) return user;

            // For Gmail SMTP, sender must match authenticated account in most cases.
            if (user.Contains("@gmail.com", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(configuredFrom, user, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }

            return configuredFrom;
        }
    }
}
