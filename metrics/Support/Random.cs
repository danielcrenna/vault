using System;
using System.Security.Cryptography;

namespace metrics.Support
{
    /// <summary>
    /// Provides statistically relevant random number generation
    /// </summary>
    public class Random
    {
        private static readonly RandomNumberGenerator _random;

        static Random()
        {
            _random = RandomNumberGenerator.Create();   
        }
        
        public static long NextLong()
        {
            var buffer = new byte[sizeof(long)];
            _random.GetBytes(buffer);
            var value = BitConverter.ToInt64(buffer, 0);
            return value;
        }
    }
}
