using System.Collections.Generic;
using System.Linq;
using email.Providers;
using email.Templates;

namespace email.Tests
{
    public class InMemoryEmailService : IEmailProvider, IEmailTemplateEngine
    {
        private readonly IEmailTemplateEngine _composer;
        public ICollection<EmailMessage> Messages { get; private set; }

        public InMemoryEmailService() : this(new DotLiquidEmailTemplateEngine())
        {
            
        }

        public InMemoryEmailService(IEmailTemplateEngine composer)
        {
            _composer = composer;
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

        public EmailMessage CreateTextEmail(string textTemplate, dynamic model)
        {
            return _composer.CreateTextEmail(textTemplate, model);
        }

        public EmailMessage CreateCombinedEmail(string htmlTemplate, string textTemplate, dynamic model)
        {
            return _composer.CreateCombinedEmail(htmlTemplate, textTemplate, model);
        }

        public EmailMessage CreateHtmlEmail(string htmlTemplate, dynamic model)
        {
            return _composer.CreateHtmlEmail(htmlTemplate, model);
        }
    }
}
