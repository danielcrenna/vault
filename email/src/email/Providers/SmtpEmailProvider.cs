using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace email.Providers
{
    /// <summary>
    /// Delivers email through the default .NET SMTP client provider
    /// </summary>
    internal class SmtpEmailProvider : IEmailProvider
    {
        private readonly Func<SmtpClient> _client;

        public SmtpEmailProvider() : this("localhost")
        {
            
        }
        public SmtpEmailProvider(string host) : this(host, 587)
        {
            
        }
        public SmtpEmailProvider(string host, int port) : this(host, port, CredentialCache.DefaultNetworkCredentials)
        {

        }
        public SmtpEmailProvider(string host, int port, ICredentialsByHost credentials)
        {
            _client = () => new SmtpClient { Host = host, Port = port, Credentials = credentials };
        }
        public SmtpEmailProvider(Func<SmtpClient> client)
        {
            _client = client;
        }
        
        public bool Send(EmailMessage message)
        {
            AlternateView textView;
            AlternateView htmlView;
            var smtpMessage = BuildMessageAndViews(message, out textView, out htmlView);
            try
            {
                _client().Send(smtpMessage);
                return true;
            }
            catch (SmtpException)
            {
                return false;
            }
            finally
            {
                if(htmlView != null)
                {
                    htmlView.Dispose();
                }
                if(textView != null)
                {
                    textView.Dispose();
                }
            }
        }

        public bool[] Send(IEnumerable<EmailMessage> messages)
        {
            var result = new List<bool>();
            foreach(var message in messages)
            {
                Send(message);
            }
            return result.ToArray();
        }

        public static MailMessage BuildMessageAndViews(EmailMessage message, out AlternateView textView, out AlternateView htmlView)
        {
            var smtpMessage = new MailMessage { BodyEncoding = Encoding.UTF8, From = new MailAddress(message.From) };
            if(message.To.Count > 0) smtpMessage.To.Add(string.Join(",", message.To));
            if(message.ReplyTo.Count > 0) smtpMessage.ReplyToList.Add(string.Join(",", message.ReplyTo));
            if(message.Cc.Count > 0) smtpMessage.CC.Add(string.Join(",", message.Cc));
            if(message.Bcc.Count > 0) smtpMessage.Bcc.Add(string.Join(",", message.Bcc));
            
            htmlView = null;
            textView = null;

            if (!string.IsNullOrWhiteSpace(message.HtmlBody))
            {
                var mimeType = new ContentType("text/html");
                htmlView = AlternateView.CreateAlternateViewFromString(message.HtmlBody, mimeType);
                smtpMessage.AlternateViews.Add(htmlView);
            }

            if (!string.IsNullOrWhiteSpace(message.TextBody))
            {
                const string mediaType = "text/plain";
                textView = AlternateView.CreateAlternateViewFromString(message.TextBody, smtpMessage.BodyEncoding, mediaType);
                textView.TransferEncoding = TransferEncoding.SevenBit;
                smtpMessage.AlternateViews.Add(textView);
            }
            return smtpMessage;
        }
    }
}