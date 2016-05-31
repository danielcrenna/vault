using System.Collections.Generic;

namespace email.Providers
{
    public interface IEmailProvider
    {
        bool Send(EmailMessage message);
        bool[] Send(IEnumerable<EmailMessage> messages);
    }
}