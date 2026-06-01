using System;
using System.Collections.Generic;
using System.Text;
using Resend;
using InfrastructureLayer.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InfrastructureLayer.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        
        public EmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var from = _configuration["Resend:FromEmail"];

            var message = new EmailMessage
            {
                From = from!,
                To = to,
                Subject = subject,
                HtmlBody = htmlBody
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
