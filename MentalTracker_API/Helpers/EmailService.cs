using System.Net.Mail;
using System.Net;

namespace MentalTracker_API.Helpers
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.mail.ru";
        private readonly int _smtpPort = 587;
        private readonly string _emailFrom = "alex_nick_schek@mail.ru";
        private readonly string _emailPassword = "VmtGRZFD59tRYe1fE1Zr";

        public async Task SendEmail(string recipientEmail, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailFrom),
                Subject = subject, 
                Body = body
            };

            mailMessage.To.Add(recipientEmail);

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort);
            smtpClient.Credentials = new NetworkCredential(_emailFrom, _emailPassword);
            smtpClient.EnableSsl = true;

            try { await smtpClient.SendMailAsync(mailMessage); }
            catch { }
        }
    }
}
