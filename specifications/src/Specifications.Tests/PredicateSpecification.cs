using System;

namespace Specifications.Tests
{
    /// <summary>
    /// Never do this and stuff; specifications are containers for explicit logic, 
    /// so littering your codebase with predicate or expression-based specifications
    /// is defeating the purpose.
    /// </summary>
    internal class PredicateSpecification<T> : SpecificationBase<T>
    {
        private readonly Predicate<T> _predicate;

        public PredicateSpecification(Predicate<T> predicate)
        {
            _predicate = predicate;
        }

        public override bool IsSatisfiedBy(T instance)
        {
            return _predicate.Invoke(instance);
        }
    }
}