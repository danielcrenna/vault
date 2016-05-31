using System;
using System.Linq;

namespace tuxedo.Extensions
{
    internal static class ObjectExtensions
    {
        public static bool Implements(this object instance, Type interfaceType)
        {
            return interfaceType.IsGenericTypeDefinition
                       ? instance.ImplementsGeneric(interfaceType)
                       : interfaceType.IsInstanceOfType(instance);
        }

        public static bool Implements<T>(this object instance)
        {
            var type = instance.GetType();
            var interfaceType = typeof(T);
            return interfaceType.IsGenericTypeDefinition
                       ? instance.ImplementsGeneric(interfaceType)
                       : interfaceType.IsAssignableFrom(type);
        }

        private static bool ImplementsGeneric(this Type type, Type targetType)
        {
            var interfaceTypes = type.GetInterfaces();
            if (interfaceTypes.Where(interfaceType => interfaceType.IsGenericType).Any(interfaceType => interfaceType.GetGenericTypeDefinition() == targetType))
            {
                return true;
            }

            var baseType = type.BaseType;
            if (baseType == null)
            {
                return false;
            }

            return baseType.IsGenericType
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
                if (interfaceType != null && interfaceType.FullName != null)
                {
                    var generic = baseType.GetInterface(interfaceType.FullName, true);
                    if (generic.IsGenericType)
                    {
                        var args = generic.GetGenericArguments();
                        if (args.Length == 1)
                        {
                            type = args[0];
                        }
                    }
                }
            }

            return type;
        }
    }
}