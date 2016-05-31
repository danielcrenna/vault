namespace Hammock.Specifications
{
    public interface ISpecification
    {

    }

    public interface ISpecification<T> : ISpecification
    {
        bool IsSatisfiedBy(T instance);
        ISpecification<T> And(ISpecification<T> other);
        ISpecification<T> Or(ISpecification<T> other);
        ISpecification<T> Not();
    }
}