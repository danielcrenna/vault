using System;
using System.Web.Mvc;
using cohort.Extensions;

namespace cohort.Mvc
{
    public static class UrlExtensions
    {
        public static string Gravatar(this UrlHelper url, string emailAddress)
        {
            const string baseUrl = "//www.gravatar.com/avatar/{0}";
            return String.Format(baseUrl, emailAddress.ToLower().MD5());
        }

        public static string Gravatar(this UrlHelper url, string emailAddress, int size)
        {
            const string baseUrl = "//www.gravatar.com/avatar/{0}?s={1}";
            return String.Format(baseUrl, emailAddress.ToLower().MD5(), size);
        }
    }
}