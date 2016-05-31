using System;
using System.Net.Http;
using System.Web;
using System.Web.Http.WebHost;

namespace cohort.API.Streaming
{
    /// <summary>
    /// <seealso href="http://www.strathweb.com/2012/09/dealing-with-large-files-in-asp-net-web-api/"/>
    /// </summary>
    public class NoBufferPolicySelector : WebHostBufferPolicySelector
    {
        public override bool UseBufferedInputStream(object hostContext)
        {
            var context = hostContext as HttpContextBase;

            if (context != null)
            {
                if (string.Equals(context.Request.RequestContext.RouteData.Values["controller"].ToString(), "uploading", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            return base.UseBufferedOutputStream(response);
        }
    }
}