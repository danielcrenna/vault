using System;
using System.Globalization;

namespace ICSharpCode.SharpZipLib.Silverlight.Compat
{
    public static class Extensions
    {
        public static int Compare(this string left, string right, bool ignoreCase, CultureInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("CultureInfo cannot be null!");
            }
            return ignoreCase
                       ? info.CompareInfo.Compare(left, right, CompareOptions.IgnoreCase)
                       : info.CompareInfo.Compare(left, right, CompareOptions.None);
        }
    }
}