using System;

namespace Hammock.Extensions
{
    internal static class FormatExtensions
    {
        // todo find an Invariant alternative for CE
        public static string ToLower(this Enum type)
        {
            return type.ToString().ToLower();
        }

        public static string ToUpper(this Enum type)
        {
            return type.ToString().ToUpper();
        }
    }
}