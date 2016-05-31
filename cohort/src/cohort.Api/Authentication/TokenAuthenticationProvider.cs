using System;
using cohort.Models;

namespace cohort.Api.Authentication
{
    public class TokenAuthenticationProvider : IAuthenticationProvider
    {
        private readonly ITokenRepository _tokenRepository;
        public TokenAuthenticationProvider(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        public string GetAuthenticationType()
        {
            return "Bearer";
        }

        public bool Authenticate(string token)
        {
            Guid value;
            if(Guid.TryParse(token, out value))
            {
                return _tokenRepository.GetByValue(value) != null;
            }
            return false;
        }
    }
}