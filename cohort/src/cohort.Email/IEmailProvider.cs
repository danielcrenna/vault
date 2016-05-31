using System.Collections.Generic;

namespace cohort.Email
{
    public interface IEmailProvider
    {
        bool Send(EmailMessage message);
        bool[] Send(IEnumerable<EmailMessage> messages);
    }
}