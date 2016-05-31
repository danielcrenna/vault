namespace cohort.Api.Authentication
{
    public interface IAuthenticationProvider
    {
        string GetAuthenticationType();
        bool Authenticate(string token);
    }
}