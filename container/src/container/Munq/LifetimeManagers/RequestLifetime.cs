// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System;
using System.Collections.Generic;

namespace Munq.LifetimeManagers
{
    public class RequestLifetime : ILifetimeManager
	{
		private const string MunqRequestLtItemsKey = "MunqRequestLtItemsKey";
        private readonly object _lock = new object();

		public static void Disposer(object sender, EventArgs e)
		{
			var application = (HttpApplication)sender;
			var _context = application.Context;
		    if (_context == null) return;

		    var requestLifetimeInstances = (Dictionary<string, object>)_context.Items[MunqRequestLtItemsKey];
		    if (requestLifetimeInstances == null) return;
		    foreach (var item in requestLifetimeInstances.Values.OfType<IDisposable>())
		    {
		        item.Dispose();
		    }
		}

		private static HttpContextBase Context
		{
			get
			{
				var context = (HttpContext.Current != null)
								? new HttpContextWrapper(HttpContext.Current)
								: null;
				return context;
			}
		}

		private static Dictionary<string, object> RequestLifetimeInstances
		{
			get
			{
				Dictionary<string, object> requestLifetimeInstances;
                if(Context != null)
                {
                    requestLifetimeInstances = Context.Items[MunqRequestLtItemsKey] as Dictionary<string, object>;
                }
                else
                {
                    requestLifetimeInstances = CallContext.GetData(MunqRequestLtItemsKey) as Dictionary<string, object>;
                }
				if (requestLifetimeInstances == null)
				{
					requestLifetimeInstances = new Dictionary<string, object>();
                    
                    if(Context != null)
                    {
                        Context.Items[MunqRequestLtItemsKey] = requestLifetimeInstances;    
                    }
                    else
                    {
                        CallContext.SetData(MunqRequestLtItemsKey, requestLifetimeInstances);
                    }
				}
				return requestLifetimeInstances;
			}
		}

		public object GetInstance(IRegistration registration)
		{
			object instance;
			if (!RequestLifetimeInstances.TryGetValue(registration.Key, out instance))
			{
				lock (_lock)
				{
					if (!RequestLifetimeInstances.TryGetValue(registration.Key, out instance))
					{
					    instance = registration.CreateInstance();
						RequestLifetimeInstances[registration.Key] = instance;
					}
				}
			}

			return instance;
		}

		/// <summary>
		/// Invalidates the cached value.
		/// </summary>
		/// <param name="registration">The Registration which is having its value invalidated</param>
		public void InvalidateInstanceCache(IRegistration registration)
		{
			RequestLifetimeInstances.Remove(registration.Key);
		}
	}
}
