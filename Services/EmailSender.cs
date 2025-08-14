using Break_Bulk_System.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

namespace Break_Bulk_System.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly MailKitEmailSender _mailKitSender;

        public EmailSender(MailKitEmailSender mailKitSender)
        {
            _mailKitSender = mailKitSender;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _mailKitSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
 }


