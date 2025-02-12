using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace CourseManagementSystem.Services.Email

{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Coursera", _config["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress(toEmail, toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = body };

            try
            {
                using (var client = new SmtpClient())
                {
                    // Cấu hình server SMTP, cổng và bảo mật
                    var smtpServer = _config["EmailSettings:SmtpServer"];
                    var port = int.TryParse(_config["EmailSettings:Port"], out var parsedPort) ? parsedPort : 587;
                    var useSsl = bool.Parse(_config["EmailSettings:UseSsl"]);

                    // Kết nối đến server SMTP với SSL hoặc TLS
                    client.Connect(smtpServer, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    // Xác thực người gửi
                    var senderEmail = _config["EmailSettings:SenderEmail"];
                    var senderPassword = _config["EmailSettings:SenderPassword"];
                    client.Authenticate(senderEmail, senderPassword);

                    // Gửi email
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết nếu có
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw new InvalidOperationException("Gửi email thất bại, vui lòng thử lại.", ex);
            }
        }
    }
}

