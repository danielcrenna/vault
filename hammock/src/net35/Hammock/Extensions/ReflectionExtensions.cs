using System.Collections.Generic;
using System.Reflection;

namespace Hammock.Extensions
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<T> GetCustomAttributes<T>(this PropertyInfo info, bool inherit)
            where T : class
        {
            var attributes = info.GetCustomAttributes(typeof (T), inherit);
            return attributes.ToEnumerable<T>();
        }

        public static object GetValue(this object instance, string property)
        {
            var info = instance.GetType().GetProperty(property);
            if (info != null)
            {
                var value = info.GetValue(instance, null);
                return value;
            }
            return null; 
        }

        public static void SetValue(this object instance, string property, object value)
        {
            var info = instance.GetType().GetProperty(property);
            info.SetValue(instance, value, null);
        }
    }
}