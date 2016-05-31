using System;

namespace ab
{
    // We might need deterministic hashes in a web farm, but the cohort itself isn't mission critical, so this should work as a default
    public class Audience
    {
        public static Lazy<Func<string, int, int>> Default = new Lazy<Func<string, int, int>>(OneBasedSplitOnHashCode);

        private static Func<string, int, int> OneBasedSplitOnHashCode()
        {
            return (identity, n) =>
            {
                var group = (int)(unchecked(((uint)identity.GetHashCode())) % n + 1);
                return group;
            };
        }
    }
}