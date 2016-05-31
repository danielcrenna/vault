using System;
using System.IO;
using System.Reflection;

namespace linger.Tests
{
    public class Utils
    {
        public static string WhereAmI()
        {
            var dir = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var fi = new FileInfo(dir.AbsolutePath);
            return fi.Directory != null ? fi.Directory.FullName : null;
        }
    }
}