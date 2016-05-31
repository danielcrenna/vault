using System;
using System.Net;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class NetworkError : RetryErrorCondition
    {
        public override Predicate<Exception> RetryIf
        {
            get
            {
                return e =>
                           {
                               var we = (e as WebException);

                               return we != null && (we.Status != WebExceptionStatus.Success &&
#if !SILVERLIGHT
                                      we.Status != WebExceptionStatus.ProtocolError &&
#endif
                                      we.Status != WebExceptionStatus.Pending);
                           };
            }
        }
    }
}