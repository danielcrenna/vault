using System;
using System.Linq;

namespace minirack.Extensions
{
    internal static class ObjectExtensions
    {
        public static bool Implements<T>(this object instance)
        {
            var interfaceType = typeof (T);
            return instance.Implements(interfaceType);
        }

        public static bool Implements(this object instance, Type interfaceType)
        {
            return interfaceType.IsGenericTypeDefinition
                       ? instance.ImplementsGeneric(interfaceType)
                       : instance.GetType().IsInstanceOfType(interfaceType);
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

        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), true).Length > 0;
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof (T), true)[0] as T;
        }
    }
}