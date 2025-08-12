using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WebApp.Configurations;

namespace WebApp.Services
{
    public class EmailService
    {
        private readonly EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            this.emailSettings = emailSettings.Value;
        }

        public async Task SendBookingConfirmationAsync(string toEmail, string employeeName, string title, string bookingId, string startDate, string endDate, string cancellationCode)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings.SenderName, emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Booking Confirmation";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <h3>Booking Confirmed</h3>
                <p>Hello {employeeName},</p>
                <p>Your booking has been successfully created.</p>
                <ul>
                    <li><strong>Booking ID:</strong> {bookingId}</li>
                    <li><strong>Title:</strong> {title}</li>
                    <li><strong>Start Date:</strong> {startDate}</li>
                    <li><strong>End Date:</strong> {endDate}</li>
                    <li><strong>Cancellation Code:</strong> {cancellationCode}</li>
                </ul>
                <p>If you need to cancel your booking, please use the cancellation code.</p>"
            };

            using var smtp = new SmtpClient();
            //await smtp.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, emailSettings.EnableSsl);
            //await smtp.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendBookingCancellationAsync(string toEmail, string employeeName, string title, string bookingId, string startDate, string endDate)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings.SenderName, emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Booking Confirmation";

            email.Body = new TextPart("html")
            {
                Text = $@"
                <h3>Booking Cancelled</h3>
                <p>Hello {employeeName},</p>
                <p>Your booking has been Cancelled.</p>
                <ul>
                    <li><strong>Booking ID:</strong> {bookingId}</li>
                    <li><strong>Title:</strong> {title}</li>
                    <li><strong>Start Date:</strong> {startDate}</li>
                    <li><strong>End Date:</strong> {endDate}</li>
                </ul>"
            };

            using var smtp = new SmtpClient();
            //await smtp.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.ConnectAsync(emailSettings.SmtpServer, emailSettings.SmtpPort, emailSettings.EnableSsl);
            //await smtp.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
