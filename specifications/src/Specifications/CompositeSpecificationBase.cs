namespace Specifications
{
    public abstract class CompositeSpecificationBase<T> : SpecificationBase<T>
    {
        protected CompositeSpecificationBase(ISpecification<T> one, ISpecification<T> other)
        {
            One = one;
            Other = other;
        }

        public ISpecification<T> One { get; private set; }
        public ISpecification<T> Other { get; private set; }
    }
}