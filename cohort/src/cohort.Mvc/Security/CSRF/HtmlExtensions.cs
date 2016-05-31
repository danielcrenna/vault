using System.Web;
using System.Web.Mvc;

namespace cohort.Security.CSRF
{
	public static class AjaxAntiForgeryHtmlExtensions
	{
		private static CsrfDataSerializer _serializer;
		internal static CsrfDataSerializer Serializer
		{
			get { return _serializer ?? (_serializer = new CsrfDataSerializer()); }
		    set
			{
				_serializer = value;
			}
		}

		public static MvcHtmlString CsrfToken(this HtmlHelper helper)
		{
			return CsrfToken(helper, null /* domain */, null /* path */);
		}

		public static MvcHtmlString CsrfToken(this HtmlHelper helper, string domain, string path)
		{
			var formValue = GetAntiForgeryTokenAndSetCookie(helper, domain, path);
			var fieldName = CsrfData.GetAntiForgeryTokenName(null);

			var builder = new TagBuilder("meta");
			builder.Attributes["name"] = fieldName;
			builder.Attributes["content"] = formValue;
			return MvcHtmlString.Create(builder.ToString(TagRenderMode.StartTag));
		}

		private static string GetAntiForgeryTokenAndSetCookie(this HtmlHelper helper, string domain, string path)
		{
			var cookieName = CsrfData.GetAntiForgeryTokenName(helper.ViewContext.HttpContext.Request.ApplicationPath);

			CsrfData cookieToken;
			var cookie = helper.ViewContext.HttpContext.Request.Cookies[cookieName];
			if (cookie != null)
			{
				cookieToken = Serializer.Deserialize(cookie.Value);
			}
			else
			{
				cookieToken = CsrfData.NewToken();
				var cookieValue = Serializer.Serialize(cookieToken);
                var newCookie = new HttpCookie(cookieName, cookieValue) { HttpOnly = true, Domain = domain };
				if (!string.IsNullOrEmpty(path))
				{
					newCookie.Path = path;
				}
				helper.ViewContext.HttpContext.Response.Cookies.Set(newCookie);
			}
			var formToken = new CsrfData(cookieToken)
			{
				Username = CsrfData.GetUsername(helper.ViewContext.HttpContext.User)
			};
			var formValue = Serializer.Serialize(formToken);
			return formValue;
		}
	}
}