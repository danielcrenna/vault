namespace cohort.Email
{
    public interface IEmailTemplateEngine
    {
        EmailMessage CreateTextEmail(string textTemplate, dynamic model);
        EmailMessage CreateCombinedEmail(string htmlTemplate, string textTemplate, dynamic model);
        EmailMessage CreateHtmlEmail(string htmlTemplate, dynamic model);
    }
}