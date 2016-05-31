namespace cohort.Services
{
    public interface ISecurityService
    {
        string Hash(string input);
        string GetNonce(int size = 32);
        bool ValidatePassword(string password, string hash);
    }
}
