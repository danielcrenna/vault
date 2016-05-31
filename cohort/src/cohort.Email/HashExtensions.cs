using System.Text;

namespace cohort.Email
{
    internal static class HashExtensions
    {
        public static string MD5(this string input)
        {
            var sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                foreach (var hex in hash)
                {
                    sb.Append(hex.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}