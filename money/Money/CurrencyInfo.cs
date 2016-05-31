using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace money
{
    /// <summary>
    /// A seamless currency class for working with money. 
    /// This class uses runtime information to separate how currency is displayed 
    /// to a user from the native currency format itself. You can create instances
    /// of CurrencyInfo implicitly using instances of <see cref="Currency"/>,
    /// <see cref="DisplayCulture"/>, or <see cref="RegionInfo" />.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Code}")]
    public partial class CurrencyInfo
    {
        private static readonly IDictionary<string, CultureInfo> Cultures;

        static CurrencyInfo()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture && !c.ThreeLetterISOLanguageName.Equals("IVL", StringComparison.InvariantCultureIgnoreCase));

            Cultures = new Dictionary<string, CultureInfo>(0);
            foreach (var culture in cultures)
            {
                Cultures.Add(culture.Name, culture);
            }
        }

        private CurrencyInfo()
        {

        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the currency code.
        /// </summary>
        /// <value>The currency code.</value>
        public Currency Code { get; private set; }

        /// <summary>
        /// Gets the native region where this currency is from.
        /// </summary>
        public RegionInfo NativeRegion { get; private set; }

        /// <summary>
        /// Gets the display culture set when this currency instance was created. 
        /// It reflects the best guess between the thread of the culture the instance
        /// was created on, and the native region of the currency itself.
        /// </summary>
        public CultureInfo DisplayCulture { get; private set; }

        /// <summary>
        /// Compares equality between this instance and a value.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns></returns>
        public bool Equals(CurrencyInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            return ReferenceEquals(this, other) || Equals(other.Code, Code);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="other">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. 
        /// </param><exception cref="T:System.NullReferenceException">The <paramref name="other"/> parameter is null.
        /// </exception><filterpriority>2</filterpriority>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.GetType() == typeof (CurrencyInfo) && Equals((CurrencyInfo) other);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(CurrencyInfo left, CurrencyInfo right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(CurrencyInfo left, CurrencyInfo right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Currency"/> to <see cref="CurrencyInfo"/>.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator CurrencyInfo(Currency currency)
        {
            var currencyInfo = Currencies[currency];

            CultureInfo fallbackCulture;

            currencyInfo.NativeRegion = GetNativeRegionFromCurrencyCodeAndUserCulture(currency, out fallbackCulture);

            currencyInfo.DisplayCulture = GetDisplayCultureFromCurrencyCodeAndUserCulture(currency);

            if (fallbackCulture != null && currencyInfo.DisplayCulture.IsNeutralCulture)
            {
                currencyInfo.DisplayCulture = fallbackCulture;
            }
            
            return currencyInfo;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Globalization.RegionInfo"/> to <see cref="CurrencyInfo"/>.
        /// </summary>
        /// <param name="regionInfo">The region info.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator CurrencyInfo(RegionInfo regionInfo)
        {
            var symbol = regionInfo.ISOCurrencySymbol;

            var currencyCode = (Currency) Enum.Parse(typeof (Currency), symbol);

            return currencyCode;
        }
        
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Globalization.CultureInfo"/> to <see cref="CurrencyInfo"/>.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator CurrencyInfo(CultureInfo cultureInfo)
        {
            return new RegionInfo(cultureInfo.LCID);
        }

        private static RegionInfo GetNativeRegionFromCurrencyCodeAndUserCulture(Enum currencyCode, out CultureInfo fallbackCulture)
        {
            // Get the current culture and region
            var userCulture = CultureInfo.CurrentCulture;
            var userLanguageName = userCulture.TwoLetterISOLanguageName;
            var userRegionName = new RegionInfo(userCulture.LCID).TwoLetterISORegionName;
            var currencySymbol = currencyCode.ToString("G");

            // Get all regions with the given currency (pivot on language to avoid losing precision)
            var locales = (from c in Cultures.Values
                           let r = new RegionInfo(c.LCID)
                           where r.ISOCurrencySymbol.Equals(currencySymbol)
                           select new {Region = r, Culture = c}).ToList();

            // Resolve the native region to the one the user is in, or the first valid one
            var locale = locales.SingleOrDefault(
                l =>
                l.Region.TwoLetterISORegionName.Equals(userRegionName) &&
                l.Culture.TwoLetterISOLanguageName.Equals(userLanguageName)
                );

            fallbackCulture = null;

            if(locale == null)
            {
                // There was no logical match for this currency in the current culture;
                // choose the most used equivalent for the native country as a fallback
                locale = locales.LastOrDefault();
                if (locale != null)
                {
                    fallbackCulture = locale.Culture;
                }
            }

            return locale == null ? null : locale.Region;
        }

        private static CultureInfo GetDisplayCultureFromCurrencyCodeAndUserCulture(Enum currencyCode)
        {
            var userCulture = CultureInfo.CurrentCulture;
            var languageCode = userCulture.TwoLetterISOLanguageName;

            CultureInfo fallbackCulture;
            var nativeRegion = GetNativeRegionFromCurrencyCodeAndUserCulture(currencyCode, out fallbackCulture);

            var cultureName = string.Format("{0}-{1}", languageCode, nativeRegion);
            var cultureInfo = Cultures.ContainsKey(cultureName)
                                  ? new CultureInfo(cultureName)
                                  : new CultureInfo(languageCode);

            return cultureInfo;
        }
    }
}