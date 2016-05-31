using System;
using System.Collections.Generic;

namespace email
{
    public class EmailMessage
    {
        public Guid Id { get; set; }
        public List<string> To { get; set; }
        public List<string> ReplyTo { get; set; }
        public string From { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }
        public string Subject { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public List<NameValuePair> Headers { get; set; }

        public bool Delivered { get; set; }
        public int DeliveryAttempts { get; set; }
        public DateTime? DeliveryTime { get; internal set; }
        public DateTime? DeliveredAt { get; set; }
        
        public EmailMessage()
        {
            Id = Guid.NewGuid();
            To = new List<string>(0);
            ReplyTo = new List<string>(0);
            Cc = new List<string>(0);
            Bcc = new List<string>(0);
            Headers = new List<NameValuePair>(0);
        }

        public void AddHeader(string name, string value)
        {
            Headers.Add(new NameValuePair {Name = name, Value = value});
        }
    }
}