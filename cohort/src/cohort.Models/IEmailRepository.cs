using cohort.Email;

namespace cohort.Models
{
    public interface IEmailRepository
    {
        void Save(EmailMessage email);
    }
}