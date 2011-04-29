using System;
using System.Security.Cryptography;
using System.Text;

namespace metrics.AspNetMvc.Extensions
{
    internal static class SecurityExtensions
    {
        public static string HashWithMd5(this string input)
        {
            var sb = new StringBuilder();
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                foreach (var hex in hash)
                {
                    sb.Append(hex.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string HashWith(this string input, HashAlgorithm algorithm)
        {
            var data = Encoding.ASCII.GetBytes(input);
            var hash = algorithm.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }
}