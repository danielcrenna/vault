using System.Collections.Generic;
using System.Linq;

namespace email.Providers
{
    /// <summary>
    /// Sends messages to a memory bucket. Probably only really useful for tests, but could be used in an intermediary queue.
    /// </summary>
    public class InMemoryEmailService : IEmailProvider 
    {
        public ICollection<EmailMessage> Messages { get; private set; }

        public InMemoryEmailService()
        {
            Messages = new List<EmailMessage>();
        }

        public bool Send(EmailMessage message)
        {
            lock(Messages)
            {
                Messages.Add(message);
                return true;
            }
        }

        public bool[] Send(IEnumerable<EmailMessage> messages)
        {
            return messages.Select(Send).ToArray();
        }
    }
}
