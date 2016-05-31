using System;
using System.Collections.Generic;
using DotLiquid;
using email.Extensions;

namespace email.Templates
{
    public class DotLiquidEmailTemplateEngine : IEmailTemplateEngine
    {
        private static readonly IDictionary<string, Template> Compiled = new Dictionary<string, Template>();

        public EmailMessage CreateTextEmail(string textTemplate, dynamic model)
        {
            var hash = HashExtensions.FromDynamic(model);
            var textBody = PrepareBodyFromTemplate(textTemplate, hash);
            var wrapped = WrapEmail(hash);
            var email = new EmailMessage
            {
                From = wrapped.From,
                To = wrapped.To,
                Subject = wrapped.Subject,
                TextBody = textBody
            };

            return email;
        }

        public EmailMessage CreateCombinedEmail(string htmlTemplate, string textTemplate, dynamic model)
        {
            var hash = HashExtensions.FromDynamic(model);
            var htmlBody = PrepareBodyFromTemplate(htmlTemplate, hash);
            var textBody = PrepareBodyFromTemplate(textTemplate, hash);

            var wrapped = WrapEmail(hash);
            var email = new EmailMessage
            {
                From = wrapped.From,
                To = wrapped.To,
                Subject = wrapped.Subject,
                TextBody = textBody,
                HtmlBody = htmlBody,
            };
            
            return email;
        }

        public EmailMessage CreateHtmlEmail(string htmlTemplate, dynamic model)
        {
            var hash = HashExtensions.FromDynamic(model);
            var htmlBody = PrepareBodyFromTemplate(htmlTemplate, hash);

            var wrapped = WrapEmail(hash);
            var email = new EmailMessage
            {
                From = wrapped.From,
                To = wrapped.To,
                Subject = wrapped.Subject,
                HtmlBody = htmlBody
            };

            return email;
        }


        private static dynamic WrapEmail(dynamic hash)
        {
            dynamic wrapped = new SafeHash((Hash)hash);
            if (!(wrapped.To is List<string>))
            {
                wrapped.To = new List<string>(new[] { wrapped.To as string });
            }
            return wrapped;
        }

        private static string PrepareBodyFromTemplate(string template, dynamic hash)
        {
            string htmlBody = null;
            if (!String.IsNullOrWhiteSpace(template))
            {
                var htmlHash = template.HashWithMd5();
                Template htmlTemplate;
                if (Compiled.ContainsKey(htmlHash))
                {
                    htmlTemplate = Compiled[htmlHash];
                }
                else
                {
                    htmlTemplate = Template.Parse(template);
                    Compiled.Add(htmlHash, htmlTemplate);
                }
                htmlBody = htmlTemplate.Render(hash);
            }
            return htmlBody;
        }
    }
}