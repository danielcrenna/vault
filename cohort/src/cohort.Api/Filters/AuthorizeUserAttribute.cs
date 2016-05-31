using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using cohort.API.Models;
using cohort.Api.Authentication;

namespace cohort.API.Filters
{
    /// <summary>
    /// Restricts access to API methods to published API authentication methods, 
    /// i.e. any registered <see cref="IAuthenticationProvider" />
    /// </summary>
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        private const string AuthorizationHeader = "Authorization";
        private const string AuthenticateHeader = "WWW-Authenticate";

        private readonly IEnumerable<IAuthenticationProvider> _providers;
        
        public AuthorizeUserAttribute()
        {
            _providers = Cohort.Container.ResolveAll<IAuthenticationProvider>();
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!actionContext.Request.Headers.Contains(AuthorizationHeader))
            {
                HandleUnauthorized(actionContext, Errors.MissingAuthorizationHeader);
                return;
            }

            var token = actionContext.Request.Headers.GetValues(AuthorizationHeader).FirstOrDefault();
            var authenticated = _providers.Aggregate(false, (current, provider) => current | provider.Authenticate(token));

            if (authenticated)
            {
                return;
            }

            HandleUnauthorizedRequest(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            HandleUnauthorized(actionContext, Errors.AuthenticationDeclined);
        }

        private void HandleUnauthorized(HttpActionContext actionContext, Error error)
        {
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, error.ToHttpError());
            foreach (var provider in _providers)
            {
                var authenticationType = provider.GetAuthenticationType();
                if (!string.IsNullOrEmpty(authenticationType))
                {
                    actionContext.Response.Headers.Add(AuthenticateHeader, authenticationType);
                }
            }
        }
    }
}