// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

using System.Web;

namespace Munq.LifetimeManagers
{
	/// <summary>
	/// A lifetime manager that scopes the lifetime of created instances to the current browser
	/// session.  An example of a class that might have Session Lifetime would be a Shopping Cart,
	/// or a multi-page entry form.
	/// </summary>
	public class SessionLifetime : ILifetimeManager
	{
		private HttpSessionStateBase _testSession;
		private readonly object _lock = new object();

		/// <summary>
		/// Gets the Session.
		/// </summary>
		private HttpSessionStateBase Session
		{
			get
			{
                var session = (HttpContext.Current != null)
                                ? new HttpSessionStateWrapper(HttpContext.Current.Session)
                                : _testSession;
				return session;
			}
		}

		#region ILifetimeManager Members
		/// <summary>
		/// Gets the instance from the Session, if available, otherwise creates a new
		/// instance and stores in the Session.
		/// </summary>
		/// <param name="registration">The creator (registration) to create a new instance.</param>
		/// <returns>The instance.</returns>
		public object GetInstance(IRegistration registration)
		{
			object instance = Session[registration.Key];
			if (instance == null)
			{
				lock (_lock)
				{
					instance = Session[registration.Key];
					if (instance == null)
					{
						instance             = registration.CreateInstance();
						Session[registration.Key] = instance;
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
			Session.Remove(registration.Key);
		}

		#endregion

		/// <summary>
		///  Only used for testing.  Has no effect when in web application
		/// </summary>
		/// <param name="context"></param>
		public void SetContext(HttpContextBase context)
		{
			_testSession = context.Session;
		}
	}
}
