using System;
using System.Collections;
using System.Security.Principal;
using System.Threading;
using System.Web.Hosting;
using HttpContextShim.WebHost;

namespace HttpContextShim
{
    public abstract class HttpContext : IHttpContext
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static volatile IHttpContext _current;
        public static IHttpContext Current
        {
            get
            {
                try
                {
                    Lock.EnterReadLock();
                    return ResolveContext();
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    Lock.EnterWriteLock();
                    _current = value;
                }
                finally
                {
                    Lock.ExitWriteLock();                    
                }
            }
        }

        private static IHttpContext ResolveContext()
        {
            var context = _current;
            if (context != null)
            {
                return context;
            }
            return context == null && HostingEnvironment.IsHosted
                       ? new AspNetHttpContext(System.Web.HttpContext.Current)
                       : null;
        }

        public DateTime Timestamp { get; protected set; }
        public IHttpRequest Request { get; protected set; }
        public IHttpResponse Response { get; protected set; }
        public IDictionary Items { get; protected set; }
        public IPrincipal User { get; protected set; }
        public object Inner { get; protected set; }
    }
}