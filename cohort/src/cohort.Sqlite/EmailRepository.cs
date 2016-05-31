using cohort.Email;
using cohort.Models;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class EmailRepository : IEmailRepository
    {
        public void Save(EmailMessage email)
        {
            UnitOfWork.Current.Insert(new Models.Email
            {
                From = email.From,
                ReplyTo = string.Join(",", email.ReplyTo),
                To = string.Join(",", email.To),
                CC = string.Join(",", email.Cc),
                BCC = string.Join(",", email.Bcc),
                Subject = email.Subject,
                TextBody = email.TextBody,
                HtmlBody = email.HtmlBody,
                DeliveredAt = email.DeliveredAt
            });
        }
    }
}