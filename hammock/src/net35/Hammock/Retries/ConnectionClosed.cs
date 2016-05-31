using System;
using System.Net;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ConnectionClosed : RetryErrorCondition
    {
        public override Predicate<Exception> RetryIf
        {
            get
            {
                return e =>
                           {
                               var we = e as WebException;
                               return we != null && (we.Status == WebExceptionStatus.ConnectionClosed || we.Status == WebExceptionStatus.KeepAliveFailure);
                           };
            }
        }
    }
}