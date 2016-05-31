using System;
using System.Globalization;

namespace money
{
    partial struct Money
    {
        public static bool operator <=(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units <= right._units;
        }

        public static bool operator >=(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units >= right._units;
        }

        public static bool operator >(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units > right._units;
        }

        public static bool operator <(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units < right._units;
        }

        public static bool operator ==(Money left, Money right)
        {
            return left._units == right._units &&
                   left._places == right._places &&
                   left._currencyInfo == right._currencyInfo;
        }

        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }

        public static Money operator +(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            left._units += right._units;

            return left;
        }

        public static Money operator -(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            left._units -= right._units;

            return left;
        }

        public static Money operator *(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            var product = Convert.ToDouble(left._units)*Convert.ToDouble(right._units);

            var factor = Math.Pow(10, left._places*2);

            product /= factor;

            var result = new Money(left._currencyInfo, product);

            return result;
        }

        public static Money operator /(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            var quotient = Convert.ToDouble(left._units)/Convert.ToDouble(right._units);

            var result = new Money(left._currencyInfo, quotient);

            return result;
        }

        public static implicit operator Money(long value)
        {
            return new Money(value);
        }

        public static implicit operator Money(double value)
        {
            return new Money(CultureInfo.CurrentCulture, value);
        }

        public static implicit operator Money(decimal value)
        {
            return new Money(CultureInfo.CurrentCulture, value);
        }

        public static implicit operator long(Money value)
        {
            return (long) value.ScaleDownToDouble();
        }

        public static implicit operator double(Money value)
        {
            return value.ScaleDownToDouble();
        }

        public static implicit operator decimal(Money value)
        {
            return value.ScaleDownToDecimal();
        }
    }
}