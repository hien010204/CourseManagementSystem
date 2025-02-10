using MailKit.Net.Smtp;
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
            emailMessage.From.Add(new MailboxAddress("Cousera", _config["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress(toEmail, toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = body };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"]);
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }

}
