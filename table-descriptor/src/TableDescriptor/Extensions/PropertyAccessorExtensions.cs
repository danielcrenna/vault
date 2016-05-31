namespace TableDescriptor.Extensions
{
    internal static class PropertyAccessorExtensions
    {
        public static bool IsPrimitiveInteger(this PropertyAccessor accessor)
        {
            var propertyType = accessor.Type;
            return propertyType == typeof(int) || propertyType == typeof(int?) || propertyType == typeof(long) || propertyType == typeof(long?);
        }
    }
}
