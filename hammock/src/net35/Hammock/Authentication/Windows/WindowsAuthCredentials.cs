using System;
using System.Net;
using Hammock.Web;

namespace Hammock.Authentication.Windows
{
#if !SILVERLIGHT
  [Serializable]
#endif

  public class WindowsAuthCredentials : IWebCredentials
  {
    private readonly ICredentials _credentials;
    public WindowsAuthCredentials(ICredentials credentials)
    {
      _credentials = credentials;
    }

    public WebQuery GetQueryFor(string url, WebParameterCollection parameters, IWebQueryInfo info, WebMethod method, bool enableTrace)
    {
      return GetQueryForImpl(info, enableTrace);
    }

    public WebQuery GetQueryFor(string url, RestBase request, IWebQueryInfo info, WebMethod method, bool enableTrace)
    {
      return GetQueryForImpl(info, enableTrace);
    }

    private WebQuery GetQueryForImpl(IWebQueryInfo info, bool enableTrace)
    {
      return new WindowsAuthWebQuery(_credentials, info, enableTrace);
    }
  }
}
