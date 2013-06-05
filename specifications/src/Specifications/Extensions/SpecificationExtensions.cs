using System;

namespace Specifications.Extensions
{
    public static class SpecificationExtensions
    {
        public static bool Satisfies<T>(this object instance) where T : ISpecification
        {
            var marker = Activator.CreateInstance<T>();
            var type = typeof(ISpecification<>).MakeGenericType(instance.GetType());
            var match = marker.Implements(type);

            if (!match)
            {
                return false;
            }

            var method = type.GetMethod("IsSatisfiedBy");
            var result = method.Invoke(marker, new[] { instance });

            return (bool)result;
        }

        public static bool Satisfies(this object instance, ISpecification specificationType)
        {
            var type = typeof(ISpecification<>).MakeGenericType(instance.GetType());
            var match = specificationType.Implements(type);

            if (!match)
            {
                return false;
            }

            var method = type.GetMethod("IsSatisfiedBy");
            var result = method.Invoke(specificationType, new[] { instance });

            return (bool)result;
        }
    }
}