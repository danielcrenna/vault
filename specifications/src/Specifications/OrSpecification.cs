namespace Specifications
{
    public class OrSpecification<T> : CompositeSpecificationBase<T>
    {
        public OrSpecification(ISpecification<T> one, ISpecification<T> other) : base(one, other) {}

        public override bool IsSatisfiedBy(T instance)
        {
            return One.IsSatisfiedBy(instance) || Other.IsSatisfiedBy(instance);
        }
    }
}