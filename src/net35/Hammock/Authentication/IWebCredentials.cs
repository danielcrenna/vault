using Hammock.Web;

namespace Hammock.Authentication
{
    public interface IWebCredentials
    {
        WebQuery GetQueryFor(string url, 
                             RestBase request, 
                             IWebQueryInfo info, 
                             WebMethod method,
                             bool enableTrace);

        WebQuery GetQueryFor(string url,
                             WebParameterCollection parameters,
                             IWebQueryInfo info,
                             WebMethod method,
                             bool enableTrace);
    }
}