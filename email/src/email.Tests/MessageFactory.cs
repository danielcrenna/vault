using System.Collections.Generic;

namespace email.Tests
{
    public static class MessageFactory
    {
        public static EmailMessage EmailWithHtmlAndText()
        {
            var message = new EmailMessage
            {
                From = "daniel.crenna@gmail.com",
                TextBody = "Hello world!",
                HtmlBody = "<html><body><p>HelloWorld!</p></body></html>"
            };
            message.To.Add("daniel.crenna@gmail.com");
            return message;
        }

        public static IEnumerable<EmailMessage> EmailWithHtmlAndText(int count)
        {
            for(var i = 0; i < count; i++)
            {
                yield return EmailWithHtmlAndText();
            }
        }

        public static EmailMessage EmailWithHtml()
        {
            var message = new EmailMessage
            {
                From = "daniel.crenna@gmail.com",
                HtmlBody = "<html><body><p>HelloWorld!</p></body></html>"
            };
            message.To.Add("daniel.crenna@gmail.com");
            return message;
        }

        public static IEnumerable<EmailMessage> EmailWithHtml(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return EmailWithHtml();
            }
        }

        public static EmailMessage EmailWithText()
        {
            var message = new EmailMessage
            {
                From = "daniel.crenna@gmail.com",
                TextBody = "Hello world!"
            };
            message.To.Add("daniel.crenna@gmail.com");
            return message;
        }

        public static IEnumerable<EmailMessage> EmailWithText(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return EmailWithText();
            }
        }
    }
}