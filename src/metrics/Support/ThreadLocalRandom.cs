using System;
using System.Threading;

namespace metrics.Support
{
    class ThreadLocalRandom
    {
        private static readonly System.Random Seeder = new System.Random();
        private static readonly ThreadLocal<int> Seed;

        private static ThreadLocal<System.Random> _random;

        static ThreadLocalRandom()
        {
            lock (Seeder)
            {
                Seed = new ThreadLocal<int>(() => Seeder.Next());
            }
        }

        public double NextNonzeroDouble()
        {
            EnsureInitialized();
            var r = _random.Value.NextDouble();
            return Math.Max(r, Double.Epsilon);
        }

        private void EnsureInitialized()
        {
            if (_random == null)
            {
                _random = new ThreadLocal<System.Random>(() => new System.Random(Seed.Value));
            }
        }
    }
}
