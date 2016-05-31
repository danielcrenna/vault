using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace money
{
    /// <summary>
    /// Money is immutable and coupled to the <see cref="CurrencyInfo" /> it belongs
    /// to at all times. In most cases, the code will attempt to determine
    /// the correct <see cref="CurrencyInfo" /> on its own based on the culture of
    /// the thread viewing the money, unless an explicit currency is provided.
    /// </summary>
    [Serializable]
    public partial struct Money : IComparable<Money>, IEquatable<Money>, IFormattable
    {
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _createdDate.GetHashCode();
                hashCode = (hashCode*397) ^ (_currencyInfo != null ? _currencyInfo.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _override.GetHashCode();
                hashCode = (hashCode*397) ^ _places.GetHashCode();
                hashCode = (hashCode*397) ^ _units.GetHashCode();
                return hashCode;
            }
        }

        private readonly DateTime _createdDate;
        private readonly CurrencyInfo _currencyInfo;
        private double? _override;
        private long _places;
        private long _units;
        
        public Money(long units) : this(CultureInfo.CurrentCulture, units)
        {

        }

        public Money(double value) : this(CultureInfo.CurrentCulture, value)
        {

        }

        public Money(decimal value) : this(CultureInfo.CurrentCulture, value)
        {

        }

        private Money(CurrencyInfo currencyInfo) : this()
        {
            _createdDate = DateTime.UtcNow;
            _currencyInfo = currencyInfo;
        }

        public Money(CurrencyInfo currencyInfo, long units) : this(currencyInfo)
        {
            _units = units;
        }

        public Money(CurrencyInfo currencyInfo, double value) : this(currencyInfo)
        {
            _units = ScaleUp(value);
        }

        public Money(CurrencyInfo currencyInfo, decimal value) : this(currencyInfo)
        {
            _units = ScaleUp(value);
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
        }

        public CurrencyInfo CurrencyInfo
        {
            get { return _currencyInfo; }
        }

        public int CompareTo(Money other)
        {
            return other._units.CompareTo(_units);
        }

        public bool Equals(Money other)
        {
            return _createdDate.Equals(other._createdDate) && Equals(_currencyInfo, other._currencyInfo) && _override.Equals(other._override) && _places == other._places && _units == other._units;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var value = ScaleDownToDouble();

            return value.ToString(format, formatProvider);
        }

        public bool Equals(double other)
        {
            return other == ScaleDownToDouble();
        }

        public bool Equals(long other)
        {
            return other == ScaleUp(ScaleDownToDouble());
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            return other is Money && Equals((Money) other);
        }

        private double ScaleDownToDouble()
        {
            if (_override.HasValue)
            {
                return _override.Value;
            }

            var numberFormat = _currencyInfo.DisplayCulture.NumberFormat;

            var scalingFactor = Math.Pow(10, _places);

            var scaled = _units/scalingFactor;

            var rounded = Math.Round(scaled, numberFormat.CurrencyDecimalDigits);

            return rounded;
        }

        private decimal ScaleDownToDecimal()
        {
            var numberFormat = _currencyInfo.DisplayCulture.NumberFormat;

            var scalingFactor = Convert.ToDecimal(Math.Pow(10, _places));

            var scaled = _units / scalingFactor;

            var rounded = Math.Round(scaled, numberFormat.CurrencyDecimalDigits);

            return rounded;
        }

        private long ScaleUp(double value)
        {
            if (double.IsInfinity(value))
            {
                _override = value;
                return long.MaxValue;
            }

            var places = CountDecimalPlaces(value);

            var scalingFactor = Math.Pow(10, places);

            var scaled = Convert.ToInt64(value*scalingFactor);

            _places = places;

            return scaled;
        }

        private long ScaleUp(decimal value)
        {
            var places = CountDecimalPlaces(value);

            var scalingFactor = Convert.ToDecimal(Math.Pow(10, places));

            var scaled = Convert.ToInt64(value * scalingFactor);

            _places = places;

            return scaled;
        }

        private static void EnsureSameCurrency(Money left, Money right)
        {
            if (left._currencyInfo != right._currencyInfo)
            {
                throw new ArithmeticException("The currency of both arguments must match to perform this operation.");
            }
        }

        private static void HarmonizeDecimalPlaces(ref Money left, ref Money right)
        {
            var scaleFactor = Math.Abs(right._places - left._places);

            if (right._places > left._places)
            {
                left._places += scaleFactor;

                left._units *= (long) Math.Pow(10, scaleFactor);
            }

            if (right._places >= left._places)
            {
                return;
            }

            right._places += scaleFactor;

            right._units *= (long) Math.Pow(10, scaleFactor);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var displayCulture = Thread.CurrentThread.CurrentCulture;
            return DisplayIn(displayCulture, false);
        }

        /// <summary>
        /// Displays the current instance as it would appear in the native culture,
        /// no matter 'where' the context thread is running.
        /// </summary>
        /// <returns></returns>
        public string DisplayNative()
        {
            return ToString("c", _currencyInfo.DisplayCulture.NumberFormat);
        }

        /// <summary>
        /// Displays the current instance as it would appear in a specified culture.
        /// </summary>
        /// <param name="displayCulture">The display culture.</param>
        /// <returns></returns>
        public string DisplayIn(CultureInfo displayCulture)
        {
            return DisplayIn(displayCulture, true);
        }

        /// <summary>
        /// Displays the value of this instance in a non-native culture, preserving
        /// the characteristics of the native <see cref="CurrencyInfo" /> but respecting 
        /// target cultural formatting.
        /// </summary>
        /// <param name="displayCulture">The culture to display this money in</param>
        /// <param name="disambiguateMatchingSymbol">If <code>true</code>, if the native culture uses the same currency symbol as the display culture, the ISO currency code is appended to the value to help differentiate the native currency.</param>
        /// <returns>A value representing this instance in another culture</returns>
        public string DisplayIn(CultureInfo displayCulture, bool disambiguateMatchingSymbol)
        {
            var sb = new StringBuilder();

            var nativeCulture = CurrencyInfo.DisplayCulture;
            if (displayCulture == nativeCulture)
            {
                return nativeCulture.ToString();
            }

            var nativeNumberFormat = nativeCulture.NumberFormat;
            nativeNumberFormat = (NumberFormatInfo) nativeNumberFormat.Clone();

            var displayNumberFormat = displayCulture.NumberFormat;
            nativeNumberFormat.CurrencyGroupSeparator = displayNumberFormat.CurrencyGroupSeparator;
            nativeNumberFormat.CurrencyDecimalSeparator = displayNumberFormat.CurrencyDecimalSeparator;

            sb.Append(ToString("c", nativeNumberFormat));

            // If the currency symbol of the display culture matches this money, add the code
            if (disambiguateMatchingSymbol && nativeNumberFormat.CurrencySymbol.Equals(displayNumberFormat.CurrencySymbol))
            {
                var currencyCode = new RegionInfo(nativeCulture.LCID).ISOCurrencySymbol;
                sb.Append(" ").Append(currencyCode);
            }

            return sb.ToString();
        }

        private static int CountDecimalPlaces(double input)
        {
            var value = input.ToString();

            var places = value.Substring(value.IndexOf('.') + 1).Length;

            return places;
        }

        private static int CountDecimalPlaces(decimal input)
        {
            var value = input.ToString(CultureInfo.InvariantCulture);

            var places = value.Substring(value.IndexOf('.') + 1).Length;

            return places;
        }
    }
}