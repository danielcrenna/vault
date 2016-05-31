// --------------------------------------------------------------------------------------------------
// © Copyright 2011 by Matthew Dennis.
// Released under the Microsoft Public License (Ms-PL) http://www.opensource.org/licenses/ms-pl.html
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Munq
{
	public partial class IocContainer : IDependencyResolver
	{
		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="Resolve1"]/*' />
		public TType Resolve<TType>() where TType : class
		{
			return Resolve(null, typeof(TType)) as TType;
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="Resolve2"]/*' />
		public TType Resolve<TType>(string name) where TType : class
		{
			return Resolve(name, typeof(TType)) as TType;
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="Resolve3"]/*' />
		public object Resolve(Type type)
		{
			return Resolve(null, type);
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="Resolve4"]/*' />
		public object Resolve(string name, Type type)
		{
			var registration = typeRegistry.Get(name, type);
            if(registration != null)
            {
                return registration.GetInstance();
            }
		    var arbitrary = HandleUnResolved(name, type);
		    return arbitrary;
		}

        private object HandleUnResolved(string name, Type type)
		{
			if (type.IsGenericType)
			{
				var result = ResolveUsingOpenType(name, type);
				if (result!=null)
				{
				    return result;
				}
			}

            if (type.IsClass)
            {
                var func = CreateInstanceDelegateFactory.Create(type);
                Register(name, type, func);
                // Thanks to dfullerton for catching this.
                return typeRegistry.Get(name, type).GetInstance();
            }

            if (type.IsInterface)
			{
				var regs = typeRegistry.GetDerived(name, type);
				var reg = regs.FirstOrDefault();
				if (reg != null)
				{
					var instance = reg.GetInstance();
					Register(name, type, (c) => c.Resolve(name, instance.GetType()));
					return instance;
				}
			}
            return null;
		}

		private object ResolveUsingOpenType(string name, Type type)
		{
            if (type.ContainsGenericParameters)
            {
                return null;
            }
            
		    // Look for an Open Type Definition registration
		    // create a type using the registered Open Type
		    // Try and resolve this type
		    var definition = type.GetGenericTypeDefinition();
		    var arguments = type.GetGenericArguments();
		    if (opentypeRegistry.ContainsKey(name, definition))
		    {
		        var reg = opentypeRegistry.Get(name, definition);
		        var implType = reg.ImplType;
		        var newType = implType.MakeGenericType(arguments);
		        try
		        {
		            if (CanResolve(name, newType))
		                return Resolve(name, newType);

		            Register(name, type, newType).WithLifetimeManager(reg.LifetimeManager);
		            return typeRegistry.Get(name, type).GetInstance();
		        }
		        catch
		        {
		            return null;
		        }
		    }
		    return null;
		}
		
		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="CanResolve1"]/*' />
		public bool CanResolve<TType>()
				where TType : class
		{
			return CanResolve(null, typeof(TType));
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="CanResolve2"]/*' />
		public bool CanResolve<TType>(string name)
				where TType : class
		{
			return CanResolve(name, typeof(TType));
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="CanResolve3"]/*' />
		public bool CanResolve(Type type)
		{
			return CanResolve(null, type);
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="CanResolve4"]/*' />
		public bool CanResolve(string name, Type type)
		{
			var @explicit = typeRegistry.ContainsKey(name, type);
		    return @explicit || Resolve(name, type) != null;
		}
		
		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="ResolveAll1"]/*' />
		public IEnumerable<TType> ResolveAll<TType>() where TType : class
		{
			return ResolveAll(typeof(TType)).Cast<TType>();
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="ResolveAll2"]/*' />
		public IEnumerable<object> ResolveAll(Type type)
		{
			var registrations = typeRegistry.GetDerived(type);
			var instances = new List<object>();
			foreach (var reg in registrations)
			{
				instances.Add(reg.GetInstance());
			}
			return instances;
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="LazyResolve1"]/*' />
		public Func<TType> LazyResolve<TType>() where TType : class
		{
			return LazyResolve<TType>(null);
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="LazyResolve2"]/*' />
		public Func<TType> LazyResolve<TType>(string name) where TType : class
		{
			return () => Resolve<TType>(name);
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="LazyResolve3"]/*' />
		public Func<Object> LazyResolve(Type type)
		{
			return LazyResolve(null, type);
		}

		/// <include file='XmlDocumentation/IDependencyResolver.xml' path='IDependencyResolver/Members[@name="LazyResolve4"]/*' />
		/// <inheritdoc />
		public Func<Object> LazyResolve(string name, Type type)
		{
			return () => Resolve(name, type);
		}

		private static string ResolveFailureMessage(Type type)
		{
			return String.Format("Munq IocContainer failed to resolve {0}", type);
		}
	}
}
