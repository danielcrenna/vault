using System;

namespace money
{
    /// <summary>
    /// Represents world currency by numeric and alphabetic values, as per ISO 4217:
    /// http://www.iso.org/iso/currency_codes_list-1. This enum is implicitly converted
    /// to <see cref="CurrencyInfo" /> instances internally, so you only need to reference this
    /// enum to work with rich currency objects. 
    /// </summary>
    [Serializable]
    public enum Currency : ushort
    {
        /// <summary>
        /// USD
        /// </summary>
        USD = 840,
        /// <summary>
        /// CAD
        /// </summary>
        CAD = 124,
        /// <summary>
        /// EUR
        /// </summary>
        EUR = 978,
        /// <summary>
        /// AUD
        /// </summary>
        AUD = 036,
        /// <summary>
        /// GBP
        /// </summary>
        GBP = 826,
        /// <summary>
        /// INR
        /// </summary>
        INR = 356,
        /// <summary>
        /// JPY
        /// </summary>
        JPY = 392,
        /// <summary>
        /// CHF
        /// </summary>
        CHF = 756,
        /// <summary>
        /// NZD
        /// </summary>
        NZD = 554,

        /// <summary>
        /// DKK
        /// </summary>
        DKK = 208

    }
}