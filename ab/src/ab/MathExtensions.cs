using System;

namespace ab
{
    internal static class MathExtensions
    {
        public static double Abs(this double value)
        {
            return Math.Abs(value);
        }

        public static double Round(this double value)
        {
            return Math.Round(value);
        }

        public static double Pow(this double value, double exp)
        {
            return Math.Pow(value, exp);
        }
    }
}