using System.IO;
using cohort.Email;
using cohort.Logging;
using cohort.Models;
using depot;

namespace cohort.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailTemplateEngine _composer;
        private readonly IEmailProvider _sender;
        private readonly IEmailRepository _repository;
        private static readonly ICache Cache;

        static EmailService()
        {
            Cache = new DefaultCache();
        }

        public EmailService(IEmailTemplateEngine composer, IEmailProvider sender, IEmailRepository repository)
        {
            _composer = composer;
            _sender = sender;
            _repository = repository;
        }

        public void Send(string templateName, dynamic model)
        {
            string textLocation;
            if (Cohort.Site.Email.Templates.TryGetValue(templateName, out textLocation))
            {
                textLocation = Cohort.Site.Email.BaseDirectory + textLocation;
                var htmlLocation = textLocation.Replace(".liquid", ".html.liquid");

                var textTemplate = LoadOrGetCachedTemplate(textLocation);
                var htmlTemplate = LoadOrGetCachedTemplate(htmlLocation);

                var hasText = textTemplate != null;
                var hasHtml = htmlTemplate != null;

                EmailMessage email = null;
                if(hasText && hasHtml)
                {
                    email = _composer.CreateCombinedEmail(htmlTemplate, textTemplate, model);
                }
                else if (hasText)
                {
                    email = _composer.CreateTextEmail(textTemplate, model);
                }
                else if (hasHtml)
                {
                    _composer.CreateHtmlEmail(htmlTemplate, model);
                }
                
                DeliverAndLog(email);
            }
            else
            {
                Logger.Warn("No email template named {0} found!");
            }
        }

        private void DeliverAndLog(EmailMessage email)
        {
            if (email != null)
            {
                _sender.Send(email);
            }
            _repository.Save(email);
        }

        private static string LoadOrGetCachedTemplate(string location)
        {
            // TODO use that funky cache func thing
            string template = Cache.Get<string>(location);
            if(template == null)
            {
                if (File.Exists(location))
                {
                    template = File.ReadAllText(location);
                    var dependency = new DefaultFileCacheDependency();
                    dependency.FilePaths.Add(location);
                    Cache.Add(location, template, dependency);
                }
            }
            return template;
        }
    }
}