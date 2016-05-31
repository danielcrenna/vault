namespace Hammock.Specifications
{
    internal class NotSpecification<T> : HammockSpecification<T>
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