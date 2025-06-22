using HRGroup.Models;
using MimeKit;

namespace HRGroup.Controllers
{ 
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(ContactU emailMessage)
        {
            var email = new MimeMessage();

            // Receiver fix
            email.To.Add(MailboxAddress.Parse("uroojmehrab286@gmail.com"));

            // Sender must be your Gmail (verified one)
            email.From.Add(MailboxAddress.Parse("uroojmehrab286@gmail.com"));

            // Set reply-to as user input
            email.ReplyTo.Add(MailboxAddress.Parse(emailMessage.From));

            email.Subject = emailMessage.Subject;

            var builder = new BodyBuilder
            {
                TextBody = $"From: {emailMessage.From}\n\n{emailMessage.Message}"
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
            await smtp.AuthenticateAsync("uroojmehrab286@gmail.com", "oqsc tcbj xnfs hlbx");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


    }
}
