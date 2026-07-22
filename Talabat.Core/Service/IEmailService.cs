using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Talabat.Core/Services.Contract/IEmailService.cs
namespace Talabat.Core.Service
    {
        public interface IEmailService
        {
            Task SendPaymentLinkAsync(string toEmail, string paymentUrl);
        }
    }

