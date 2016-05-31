using System;
using TableDescriptor;

namespace cohort.Models
{
    public class Email
    {
        public string From { get; set; }
        public string ReplyTo { get; set; }
        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [Computed]
        public DateTime CreatedAt { get; set; }
    }
}