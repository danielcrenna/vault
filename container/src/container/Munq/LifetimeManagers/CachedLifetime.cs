// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.Caching;

namespace Munq.LifetimeManagers
{
	/// <summary>
	/// A Lifetime Manager that uses the Cache to store the instance
	/// </summary>
	/// <remarks>
	/// The cache can be invalidated at any time.  After that, the next Resolve will create a new 
	/// instance will be created and cached.  Don't assume that instances are the same.
	/// </remarks>
	/// <example>
	/// This example uses the IocContainer to cache RSS feeds for 15 minutes, and reloads on the
	/// next request after that.
	/// <code>
	///      var container = new IocContainer();
	///      var cachedFor15Minutes = new CachedLifetime().ExpiresAfter(new TimeSpan(0, 15, 0));
	///      
	///		 container.Register&lt;IRssFeed&gt;("Munq",    c => new RssFeed("http://munq.codeplex.com/Project/ProjectRss.aspx"))
	///		          .WithLifetimeManager(cachedFor15Minutes);
	///		          
	///		 container.Register&lt;IRssFeed&gt;("TheGu",   c => new RssFeed("http://weblogs.asp.net/scottgu/rss.aspx"))
	///		          .WithLifetimeManager(cachedFor15Minutes);
	///		          
	///		 container.Register&lt;IRssFeed&gt;("Haacked", c => new RssFeed("http://feeds.haacked.com/haacked/"))
	///		          .WithLifetimeManager(cachedFor15Minutes);
	///		 ...         
	///      var feed = container.Resolve&lt;IRssFeed&gt;("TheGu");
	/// </code>
	/// </example>
	public class CachedLifetime : ILifetimeManager, IDisposable
	{
		private enum Expires
		{
			None,
			OnDateTime,
			AfterFixedDuration,
			AfterSlidingDuration
		};
		private Expires  _expirationKind     = Expires.None;
		private DateTime _expiresOn          = Cache.NoAbsoluteExpiration;
		private TimeSpan _duration           = Cache.NoSlidingExpiration;
		private CacheItemPriority _priority  = CacheItemPriority.Default;
		private CacheDependency _dependencies;
		private CacheItemRemovedCallback _onRemoveCallback;
		private readonly object _lock = new object();

		#region ILifetimeManager Members
		/// <summary>
		/// Gets the instance from cache, if available, otherwise creates a new
		/// instance and caches it.
		/// </summary>
		/// <param name="registration">The creator (registration) to create a new instance.</param>
		/// <returns>The instance.</returns>
		public object GetInstance(IRegistration registration)
		{
			Cache cache = HttpRuntime.Cache;

			string key = registration.Key;
			object instance = cache[key];
			if (instance == null)
			{
				lock (_lock)
				{
					instance = cache[key];
					if (instance == null)
					{
						instance = registration.CreateInstance();

						if (_expiresOn == Cache.NoAbsoluteExpiration &&
							_duration == Cache.NoSlidingExpiration)
							_expirationKind = Expires.None;

						switch (_expirationKind)
						{
							case Expires.None:
								cache.Insert(key, instance, _dependencies, Cache.NoAbsoluteExpiration,
										Cache.NoSlidingExpiration, _priority, _onRemoveCallback);
								break;

							case Expires.OnDateTime:
								cache.Insert(key, instance, _dependencies, _expiresOn,
										Cache.NoSlidingExpiration, _priority, _onRemoveCallback);
								break;

							case Expires.AfterFixedDuration:
								cache.Insert(key, instance, _dependencies, DateTime.Now.Add(_duration),
										Cache.NoSlidingExpiration, _priority, _onRemoveCallback);
								break;

							case Expires.AfterSlidingDuration:
								cache.Insert(key, instance, _dependencies, Cache.NoAbsoluteExpiration,
									_duration, _priority, _onRemoveCallback);
								break;
						}

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
			Cache cache = HttpRuntime.Cache;
			cache.Remove(registration.Key);
		}

		#endregion

		/// <summary>
		/// Sets the Cache Dependencies for this LifetimeManager.
		/// </summary>
		/// <param name="dependencies">The CacheDependencies.</param>
		/// <returns>The CachedLifetime instance (allows chaining).</returns>
		public CachedLifetime IsDependentOn(CacheDependency dependencies)
		{
			_dependencies = dependencies;
			return this;
		}

		/// <summary>
		/// Sets the absolute time when the cached value expires.
		/// </summary>
		/// <param name="absoluteExpiration">The date/time when the item expires.</param>
		/// <returns>The CachedLifetime instance (allows chaining).</returns>
		public CachedLifetime ExpiresOn(DateTime absoluteExpiration)
		{
			if (absoluteExpiration != Cache.NoAbsoluteExpiration)
			{
				_duration       = Cache.NoSlidingExpiration;
				_expirationKind = Expires.OnDateTime;
			}

			_expiresOn = absoluteExpiration;
			return this;
		}

		/// <summary>
		/// Sets the duration the cached item will remain valid.  This is a sliding duration.
		/// </summary>
		/// <param name="duration">The duration. Use Cache.NoSlidingExpiration to disable.</param>
		/// <returns>The CachedLifetime instance (allows chaining).</returns>
		public CachedLifetime ExpiresAfterNotAccessedFor(TimeSpan duration)
		{
			if (duration != Cache.NoSlidingExpiration)
			{
				_expiresOn      = Cache.NoAbsoluteExpiration;
				_expirationKind = Expires.AfterSlidingDuration;
			}

			_duration = duration;
			return this;
		}

		/// <summary>
		/// Sets the duration the cached item will remain valid.  This is a fixed duration.
		/// </summary>
		/// <param name="duration">The duration. Use Cache.NoSlidingExpiration to disable.</param>
		/// <returns>The CachedLifetime instance (allows chaining).</returns>
		public CachedLifetime ExpiresAfter(TimeSpan duration)
		{
			if (duration != Cache.NoSlidingExpiration)
			{
				_expiresOn      = Cache.NoAbsoluteExpiration;
				_expirationKind = Expires.AfterFixedDuration;
			}

			_duration = duration;
			return this;
		}

		/// <summary>
		/// Sets the priority of the item in the cache.
		/// </summary>
		/// <param name="priority">The priority.</param>
		/// <returns>The CachedLifetime instance (allows chaining).</returns>

		public CachedLifetime WithPriority(CacheItemPriority priority)
		{
			_priority = priority;
			return this;
		}

		/// <summary>
		/// Sets a callback method for when an item is removed (expires).
		/// </summary>
		/// <param name="onRemoveCallback">The callback method.</param>
		/// <returns>The CachedLifetime instance (allows chaining).</returns>
		public CachedLifetime CallbackOnRemoval(CacheItemRemovedCallback onRemoveCallback)
		{
			_onRemoveCallback = onRemoveCallback;
			return this;
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				if (_dependencies != null)
				{
					_dependencies.Dispose();
					_dependencies = null;
				}
		}
		~CachedLifetime()
		{
			Dispose(false);
		}
	}
}
