using System.Collections.Generic;

namespace email
{
    public interface IEmailService
    {
        void Send(EmailMessage message);
        void Send(IEnumerable<EmailMessage> messages);
    }
}