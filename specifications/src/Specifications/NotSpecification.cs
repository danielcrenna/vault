namespace Specifications
{
    public class NotSpecification<T> : SpecificationBase<T>
    {
        private readonly ISpecification<T> _original;

        public NotSpecification(ISpecification<T> original)
        {
            _original = original;
        }

        public override bool IsSatisfiedBy(T instance)
        {
            return !_original.IsSatisfiedBy(instance);
        }
    }
}