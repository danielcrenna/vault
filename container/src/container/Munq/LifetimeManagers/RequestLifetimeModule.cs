using System;
using System.Web;

namespace Munq.LifetimeManagers
{
	public class RequestLifetimeModule : IHttpModule
	{
		/// <summary>
		/// You will need to configure this module in the web.config file of your
		/// web and register it with IIS before being able to use it. For more information
		/// see the following link: http://go.microsoft.com/?linkid=8101007
		/// </summary>
		#region IHttpModule Members

		public void Dispose()
		{
			//clean-up code here.
		}

		public void Init(HttpApplication context)
		{
			// Below is an example of how you can handle LogRequest event and provide 
			// custom logging implementation for it
			context.EndRequest += new EventHandler(context_EndRequest);
		}

		void context_EndRequest(object sender, EventArgs e)
		{
			RequestLifetime.Disposer(sender, e);
		}

		#endregion

	}
}
