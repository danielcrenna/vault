using System;
using System.Threading;

namespace Hammock.Web
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class WebQueryAsyncResult : IAsyncResult, IDisposable
    {
        public virtual bool IsCompleted { get; protected internal set; }
        public virtual WaitHandle AsyncWaitHandle { get; protected internal set; }
        public virtual object AsyncState { get; protected internal set; }
        public virtual bool CompletedSynchronously { get; protected internal set; }

        public virtual IAsyncResult InnerResult { get; set; }
        public virtual object Tag { get; set; }

#if !SILVERLIGHT
        [NonSerialized]
#endif
        private AutoResetEvent _block;

        public WebQueryAsyncResult()
        {
            Initialize();
        }

        private void Initialize()
        {
            _block = new AutoResetEvent(false);
            AsyncWaitHandle = _block;
        }

        protected internal void Signal()
        {
            _block.Set();
        }

        public void Dispose()
        {
            
        }
    }
}
