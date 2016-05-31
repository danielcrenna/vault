using System.Collections.Specialized;
using System;
using System.Web;

namespace minirack
{
    public class AppHarborModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            Action<NameValueCollection, bool> setReadOnly;
            var isReadOnly = AppHarbor.InitializeExpressions(out setReadOnly);
            context.BeginRequest += (sender, args) => AppHarbor.RemapForAppHarbor(isReadOnly, setReadOnly);
        }

        public void Dispose()
        {

        }
    }
}