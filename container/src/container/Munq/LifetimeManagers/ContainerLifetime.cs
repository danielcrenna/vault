// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

namespace Munq.LifetimeManagers
{
	/// <summary>
	/// A lifetime manager that scopes the lifetime of created instances to the lifetime of the
	/// container.
	/// </summary>
	public class ContainerLifetime : ILifetimeManager
	{
		#region ILifetimeManager Members
		/// <summary>
		/// Gets the instance from the container instance cache, if available, otherwise creates a new
		/// instance and caches it. This effectively make the instance an application scoped Singleton.
		/// </summary>
		/// <param name="registration">The creator (registration) to create a new instance.</param>
		/// <returns>The instance.</returns>
		public object GetInstance(IRegistration registration)
		{
			return (registration as Registration).GetCachedInstance();
		}

		/// <summary>
		/// Invalidates the cached value.
		/// </summary>
		/// <param name="registration">The Registration which is having its value invalidated</param>
		public void InvalidateInstanceCache(IRegistration registration)
		{
			var reg = registration as Registration;
			if (reg != null)
				reg.Instance = null;
		}

		#endregion

	}
}
