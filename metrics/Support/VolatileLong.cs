using System.Threading;

namespace metrics.Support
{
    /// <summary>
    /// Provides support for volatile operations around a <see cref="long" /> value
    /// </summary>
    public struct VolatileLong
    {
        private long _value;

        public static VolatileLong operator +(VolatileLong left, VolatileLong right)
        {
            return Add(left, right);
        }

        private static VolatileLong Add(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() + right.Get());
            return left.Get();
        }

        public static VolatileLong operator -(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() - right.Get());
            return left.Get();
        }

        public static VolatileLong operator *(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() * right.Get());
            return left.Get();
        }

        public static VolatileLong operator /(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() / right.Get());
            return left.Get();
        }

        private VolatileLong(VolatileLong value): this()
        {
            Set(value);
        }

        public void Set(long value)
        {
            Thread.VolatileWrite(ref _value, value);
        }

        public long Get()
        {
            return Thread.VolatileRead(ref _value);
        }

        public static implicit operator VolatileLong(long value)
        {
            return new VolatileLong(value);
        }

        public static implicit operator long(VolatileLong value)
        {
            return value.Get();
        }

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}