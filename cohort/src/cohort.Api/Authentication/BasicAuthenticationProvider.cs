using System;
using System.Text;
using cohort.Services;
using cohort.ViewModels;

namespace cohort.Api.Authentication
{
    public class BasicAuthenticationProvider : IAuthenticationProvider
    {
        private readonly AccountService _accountService;

        public BasicAuthenticationProvider(AccountService accountService)
        {
            _accountService = accountService;
        }

        public string GetAuthenticationType()
        {
            return "Basic";
        }

        public bool Authenticate(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
            var encodedDataAsBytes = Convert.FromBase64String(token.Replace("Basic ", string.Empty));
            var value = Encoding.ASCII.GetString(encodedDataAsBytes);
            var userpass = value;
            
            var user = userpass.Substring(0, userpass.IndexOf(':'));
            var password = userpass.Substring(userpass.IndexOf(':') + 1);

            var identity = _accountService.GetUser(user, true);
            if(identity == null)
            {
                return false;
            }

            var model = new SignIn {Email = user, Password = password};
            return _accountService.SignInIsValid(model, identity);
        }
    }
}