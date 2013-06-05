using System;
using System.Net;

namespace Hammock.Web
{
  /// <summary>
  /// A web query engine for making requests that use windows authorization.
  /// </summary>
  public class WindowsAuthWebQuery : WebQuery
  {
    private readonly ICredentials _credentials;
    public WindowsAuthWebQuery(ICredentials credentials, IWebQueryInfo info, bool enableTrace)
      : base(info, enableTrace)
    {
      _credentials = credentials;
    }

    protected override void AuthenticateRequest(WebRequest request)
    {
      request.Credentials = _credentials;
    }

    public override string GetAuthorizationContent()
    {
      throw new NotImplementedException();
    }

    protected override void SetAuthorizationHeader(WebRequest request, string header)
    {
      throw new NotImplementedException();
    }
  }
}
