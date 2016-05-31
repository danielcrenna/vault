using System;
using System.Linq;

namespace TableDescriptor.Extensions
{
    internal static class AttributeExtensions
    {
        public static bool Has<T>(this PropertyAccessor accessor) where T : Attribute
        {
            Attribute exists = accessor.Attributes.OfType<T>().FirstOrDefault();
            return exists != null;
        }
    }
}
