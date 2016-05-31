using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace email.Providers
{
    /// <summary>
    /// Delivers email as .EML files to a specified directory.
    /// </summary>
    public class DirectoryEmailProvider : IEmailProvider
    {
        private readonly Func<SmtpClient> _client;

        public DirectoryEmailProvider(string directory)
        {
            _client = () => new SmtpClient
            {
                Host = "localhost",
                Credentials = CredentialCache.DefaultNetworkCredentials,
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = directory
            };
        }

        public bool Send(EmailMessage message)
        {
            AlternateView textView;
            AlternateView htmlView;
            var smtpMessage = SmtpEmailProvider.BuildMessageAndViews(message, out textView, out htmlView);
            try
            {
                _client().Send(smtpMessage);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (htmlView != null)
                {
                    htmlView.Dispose();
                }
                if (textView != null)
                {
                    textView.Dispose();
                }  
            }
        }

        public bool[] Send(IEnumerable<EmailMessage> messages)
        {
            var result = new List<bool>();
            foreach (var message in messages)
            {
                Send(message);
            }
            return result.ToArray();
        }
    }
}