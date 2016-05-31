// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Munq.LifetimeManagers
{
    /// <summary>
    /// A LifetimeManager that uses Thread Local Storage to cache instances.
    /// </summary>
    public class ThreadLocalStorageLifetime : ILifetimeManager
    {
        // The thread local storage.  The ThreadStatic attribute makes this easy.
        [ThreadStatic]
        static Dictionary<string, object> localStorage;

        /// <summary>
        /// Gets an instance from the thread local storage, or creates a new instance if not found.
        /// </summary>
        /// <param name="creator">The IInstanceCreate to use to get the Key and create new if required.</param>
        /// <returns>The instance.</returns>
        public object GetInstance(IRegistration creator)
        {
            object instance = null;

            // if it is a new thread then the localStorage needs to be initialized;
            if (localStorage == null)
                localStorage = new Dictionary<string,object>();
 
            if (!localStorage.TryGetValue(creator.Key, out instance))
            {
                instance                  = creator.CreateInstance();
                localStorage[creator.Key] = instance;
            }

            return instance;
        }

        /// <summary>
        /// Removes the instance for the registration from the local storage cache.
        /// </summary>
        /// <param name="registration">The IRegistration returned when the type was registered in the IOC container.</param>
        public void InvalidateInstanceCache(IRegistration registration)
        {
            // nothing stored yet
            if (localStorage == null)
                return;

            localStorage.Remove(registration.Key);
        }
    }
}
