using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
