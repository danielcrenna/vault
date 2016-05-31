using System;
using System.Reflection;
using System.Web.Mvc;
using cohort.Security.CSRF;

namespace cohort.Mvc.Security.CSRF
{
    /// <summary>
    /// A custom CSRF validation attribute that works on both regular HTTP forms and AJAX requests
    /// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AuthenticHttpPost : ActionMethodSelectorAttribute, IAuthorizationFilter
	{
        private string _salt;
		private CsrfDataSerializer _serializer;

		public string Salt
		{
			get
			{
				return _salt ?? string.Empty;
			}
			set
			{
				_salt = value;
			}
		}

		internal CsrfDataSerializer Serializer
		{
			get { return _serializer ?? (_serializer = new CsrfDataSerializer()); }
		    set
			{
				_serializer = value;
			}
		}

		private bool ValidateFormToken(CsrfData token)
		{
			return (String.Equals(Salt, token.Salt, StringComparison.Ordinal));
		}

		private static HttpAntiForgeryException CreateValidationException()
		{
			return new HttpAntiForgeryException("A required anti-forgery token was not supplied or was invalid.");
		}

		public void OnAuthorization(AuthorizationContext filterContext)
		{
			if (filterContext == null)
			{
				throw new ArgumentNullException("filterContext");
			}

			var cookieName = CsrfData.GetAntiForgeryTokenName(filterContext.HttpContext.Request.ApplicationPath);
            var cookie = filterContext.HttpContext.Request.Cookies[cookieName];
			if (cookie == null || string.IsNullOrEmpty(cookie.Value))
			{
				// error: cookie token is missing
				throw CreateValidationException();
			}
			var cookieToken = Serializer.Deserialize(cookie.Value);

            // Token is either in a traditional form POST or in an AJAX 
            var formValue = filterContext.HttpContext.Request.IsAjaxRequest()
                                   ? filterContext.HttpContext.Request.Headers[CsrfData.GetAntiForgeryTokenHeaderName()]
                                   : filterContext.HttpContext.Request[CsrfData.GetAntiForgeryTokenName(null)];

			if (string.IsNullOrEmpty(formValue))
			{
				// error: form token is missing
				throw CreateValidationException();
			}
			var formToken = Serializer.Deserialize(formValue);

			if (!string.Equals(cookieToken.Value, formToken.Value, StringComparison.Ordinal))
			{
				// error: form token does not match cookie token
				throw CreateValidationException();
			}

			var currentUsername = CsrfData.GetUsername(filterContext.HttpContext.User);
			if (!String.Equals(formToken.Username, currentUsername, StringComparison.OrdinalIgnoreCase))
			{
				// error: form token is not valid for this user
				// (don't care about cookie token)
				throw CreateValidationException();
			}

			if (!ValidateFormToken(formToken))
			{
				// error: custom validation failed
				throw CreateValidationException();
			}
		}

        private static readonly AcceptVerbsAttribute Inner = new AcceptVerbsAttribute(HttpVerbs.Post);
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return Inner.IsValidForRequest(controllerContext, methodInfo);
        }
	}
}