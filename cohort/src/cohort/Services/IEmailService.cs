namespace cohort.Services
{
    public interface IEmailService
    {
        void Send(string templateName, dynamic model);
    }
}