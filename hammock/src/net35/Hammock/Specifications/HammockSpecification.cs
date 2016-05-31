namespace Hammock.Specifications
{
    public abstract class HammockSpecification<T> : ISpecification<T>
    {
        #region ISpecification<T> Members

        public abstract bool IsSatisfiedBy(T instance);

        public virtual ISpecification<T> And(ISpecification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        public virtual ISpecification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        public virtual ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }

        #endregion

        public static ISpecification<T> operator &(HammockSpecification<T> one, ISpecification<T> other)
        {
            return one.And(other);
        }

        public static ISpecification<T> operator |(HammockSpecification<T> one, ISpecification<T> other)
        {
            return one.Or(other);
        }
    }
}