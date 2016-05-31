using System;
using System.Linq;

#if WINRT
using System.Reflection;
using TweetSharp.WinRT.Compat;
#endif

namespace TweetSharp
{
    internal static class ObjectExtensions
    {
        public static bool EqualsAny(this object instance, params object[] args)
        {
            return args.Aggregate(false, (current, arg) => current | instance.Equals(arg));
        }

        public static bool Implements(this object instance, Type interfaceType)
        {
#if !WINRT
            return interfaceType.IsGenericTypeDefinition
                       ? instance.ImplementsGeneric(interfaceType)
                       : interfaceType.IsAssignableFrom(instance.GetType());
#else
					var ti = interfaceType.GetTypeInfo();
					return ti.IsGenericTypeDefinition
										 ? instance.ImplementsGeneric(interfaceType)
										 : ti.IsAssignableFrom(instance.GetType().GetTypeInfo());
#endif
				}

        public static bool Implements<T>(this object instance)
        {
#if !WINRT
            var type = instance.GetType();
            var interfaceType = typeof (T);
            return interfaceType.IsGenericTypeDefinition
                       ? instance.ImplementsGeneric(interfaceType)
                       : interfaceType.IsAssignableFrom(type);
#else
					var type = instance.GetType();
					var interfaceType = typeof(T);
					var ti = type.GetTypeInfo();
					return ti.IsGenericTypeDefinition
										 ? instance.ImplementsGeneric(interfaceType)
										 : ti.IsAssignableFrom(type.GetTypeInfo());
#endif
				}

        private static bool ImplementsGeneric(this Type type, Type targetType)
        {
#if !WINRT
            var interfaceTypes = type.GetInterfaces();
#else
					var interfaceTypes = type.GetTypeInfo().ImplementedInterfaces;
#endif
            foreach (var interfaceType in interfaceTypes)
            {
#if !WINRT
                if (!interfaceType.IsGenericType)
#else
							if (!interfaceType.GetTypeInfo().IsGenericType)
#endif
                {
                    continue;
                }

                if (interfaceType.GetGenericTypeDefinition() == targetType)
                {
                    return true;
                }
            }

#if !WINRT
            var baseType = type.BaseType;
#else
						var baseType = type.GetTypeInfo().BaseType;
#endif
            if (baseType == null)
            {
                return false;
            }

#if !WINRT
            return baseType.IsGenericType
#else
						return baseType.GetTypeInfo().IsGenericType
#endif
                       ? baseType.GetGenericTypeDefinition() == targetType
                       : baseType.ImplementsGeneric(targetType);
        }

        private static bool ImplementsGeneric(this object instance, Type targetType)
        {
            return instance.GetType().ImplementsGeneric(targetType);
        }

        public static Type GetDeclaredTypeForGeneric(this object instance, Type interfaceType)
        {
            return instance.GetType().GetDeclaredTypeForGeneric(interfaceType);
        }

        public static Type GetDeclaredTypeForGeneric(this Type baseType, Type interfaceType)
        {
            var type = default(Type);

            if (baseType.ImplementsGeneric(interfaceType))
            {
                var generic = baseType.GetInterface(interfaceType.FullName, true);
#if !WINRT
                if (generic.IsGenericType)
#else
								var ti = generic.GetTypeInfo();
								if (ti.IsGenericType)
#endif

								{
#if !WINRT
                    var args = generic.GetGenericArguments();
#else
									var args = ti.GenericTypeArguments;
#endif
                    if (args.Length == 1)
                    {
                        type = args[0];
                    }
                }
            }

            return type;
        }
    }
}