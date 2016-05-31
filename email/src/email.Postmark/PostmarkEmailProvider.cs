using System;
using System.Collections.Generic;
using System.Linq;
using PostmarkDotNet;
using email.Providers;

namespace email.Postmark
{
    public class PostmarkEmailProvider : IEmailProvider  
    {
        private const int BatchMax = 500;

        private readonly PostmarkClient _client;

        public PostmarkEmailProvider(string serverToken)
        {
            _client = new PostmarkClient(serverToken);
        }

        public bool Send(EmailMessage message)
        {
            message.DeliveryAttempts++;
            var pm = CreatePostmarkMessage(message);
            var response = _client.SendMessage(pm);
            message.Delivered = response.Status == PostmarkStatus.Success;
            if(message.Delivered)
            {
                message.DeliveredAt = DateTime.UtcNow;
            }
            return message.Delivered;
        }

        private static PostmarkMessage CreatePostmarkMessage(EmailMessage message)
        {
            var pm = new PostmarkMessage
            {
                From = message.From,
                Subject = message.Subject,
                TextBody = message.TextBody,
                HtmlBody = message.HtmlBody,
                To = message.To.Count > 0 ? string.Join(",", message.To) : "",
                ReplyTo = message.ReplyTo.Count > 0 ? string.Join(",", message.ReplyTo) : "",
                Cc = message.Cc.Count > 0 ? string.Join(",", message.Cc) : "",
                Bcc = message.Bcc.Count > 0 ? string.Join(",", message.Bcc) : ""
            };
            foreach(var header in message.Headers)
            {
                pm.Headers.Add(header.Name, header.Value);
            }
            return pm;
        }

        public bool[] Send(IEnumerable<EmailMessage> messages)
        {
            // Postmark only allows 500 messages per batch, so we'll send successive batches
            var payload = messages.ToList();
            var results = new List<bool>();
            if (payload.Count > BatchMax)
            {
                var batches = (int)Math.Ceiling(payload.Count / (double)BatchMax);
                for(var i = 0; i < batches; i++)
                {
                    var slice = payload.Skip(i * BatchMax).Take(BatchMax).ToList();
                    results.Add(SendBatch(slice));
                }
            }
            return results.ToArray();
        }

        private bool SendBatch(IList<EmailMessage> payload)
        {
            var pm = payload.Select(CreatePostmarkMessage).ToList();
            var responses = _client.SendMessages(pm).ToList();
            for (var i = 0; i < responses.Count(); i++)
            {
                payload[i].DeliveryAttempts++;
                if (responses[i].Status != PostmarkStatus.Success)
                {
                    continue;
                }
                payload[i].Delivered = true;
                payload[i].DeliveredAt = DateTime.UtcNow;
            }
            return responses.All(r => r.Status == PostmarkStatus.Success);
        }
    }
}
