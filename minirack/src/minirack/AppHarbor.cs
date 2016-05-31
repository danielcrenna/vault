using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace minirack
{
    /// <remarks>
    /// Original: https://raw.github.com/trilobyte/Premotion-AspNet-AppHarbor-Integration/master/src/Premotion.AspNet.AppHarbor.Integration/AppHarborIntegrationModule.cs
    /// http://support.appharbor.com/kb/getting-started/workaround-for-generating-absolute-urls-without-port-number
    /// http://support.appharbor.com/kb/getting-started/information-about-our-load-balancer
    /// </remarks>
    public class AppHarbor
    {
        private const string AppHarborDetectionSettingKey = "appharbor.commit_id";
        private const string ForwardedForHeaderName = "HTTP_X_FORWARDED_FOR";
        private const string ForwardedProtocolHeaderName = "HTTP_X_FORWARDED_PROTO";
        private const string ForwardedForAddressesSeparator = ", ";
        
        public static bool IsHosting()
        {
            var appHarborCommitId = ConfigurationManager.AppSettings[AppHarborDetectionSettingKey];
            return !string.IsNullOrEmpty(appHarborCommitId);
        }

        public static Func<NameValueCollection, bool> InitializeExpressions(out Action<NameValueCollection, bool> setReadOnly)
        {
            var collectionType = typeof (NameValueCollection);
            var readOnlyProperty = collectionType.GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            if (readOnlyProperty == null)
            {
                throw new InvalidOperationException(string.Format("Could not find property '{0}' on type '{1}'", "IsReadOnly", collectionType));
            }
            var collectionParam = Expression.Parameter(typeof (NameValueCollection));
            var isReadOnly = Expression.Lambda<Func<NameValueCollection, bool>>(Expression.Property(collectionParam, readOnlyProperty), collectionParam).Compile();
            var valueParam = Expression.Parameter(typeof(bool));
            setReadOnly = Expression.Lambda<Action<NameValueCollection, bool>>(Expression.Call(collectionParam, readOnlyProperty.GetSetMethod(true), valueParam), collectionParam, valueParam).Compile();
            return isReadOnly;
        }

        public static void RemapForAppHarbor(Func<NameValueCollection, bool> isReadOnly, Action<NameValueCollection, bool> setReadOnly)
        {
            var serverVariables = HttpContext.Current.Request.ServerVariables;
            var wasReadOnly = isReadOnly(serverVariables);
            if (wasReadOnly)
            {
                setReadOnly(serverVariables, false);
            }

            var forwardedFor = serverVariables[ForwardedForHeaderName] ?? string.Empty;
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var forwardSeparatorIndex = forwardedFor.LastIndexOf(ForwardedForAddressesSeparator, StringComparison.Ordinal);
                if (forwardSeparatorIndex < 0)
                {
                    serverVariables.Set("REMOTE_ADDR", forwardedFor);
                    serverVariables.Remove(ForwardedForHeaderName);
                }
                else
                {
                    serverVariables.Set("REMOTE_ADDR", forwardedFor.Substring(forwardSeparatorIndex + ForwardedForAddressesSeparator.Length));
                    serverVariables.Set(ForwardedForHeaderName, forwardedFor.Remove(forwardSeparatorIndex));
                }
            }

            var protocol = serverVariables[ForwardedProtocolHeaderName];
            if (!string.IsNullOrEmpty(protocol))
            {
                serverVariables.Remove(ForwardedProtocolHeaderName);

                var isHttps = "HTTPS".Equals(protocol, StringComparison.OrdinalIgnoreCase);
                serverVariables.Set("HTTPS", isHttps ? "on" : "off");
                serverVariables.Set("SERVER_PORT", isHttps ? "443" : "80");
                serverVariables.Set("SERVER_PORT_SECURE", isHttps ? "1" : "0");
            }

            var isAjaxFlag = serverVariables["HTTP_X_REQUESTED_WITH"];
            if (!string.IsNullOrEmpty(isAjaxFlag))
            {
                serverVariables.Set("X-Requested-With", isAjaxFlag);
            }

            if (wasReadOnly)
            {
                setReadOnly(serverVariables, true);
            }
        }
    }
}