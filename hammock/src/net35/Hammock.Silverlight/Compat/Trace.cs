using System;
using System.Diagnostics;
using System.Text;

namespace Hammock.Silverlight.Compat
{
    public class Trace
    {
        public static bool Enabled { get; set; }

        static Trace()
        {
            Enabled = true;
        }

        public static void WriteLine(string message)
        {
            if (!Enabled)
            {
                return;
            }

            Debug.WriteLine(message);
        }

        public static void WriteLineIf(bool condition, string message)
        {
            if (!Enabled)
            {
                return;
            }

            if(condition)
            {
                Debug.WriteLine(message);
            }
        }

        public static void WriteLine(string message, params object[] args)
        {
            if (!Enabled)
            {
                return;
            }

            Debug.WriteLine(message, args);
        }

        public static void WriteLine(StringBuilder sb)
        {
            if (!Enabled)
            {
                return;
            }

            Debug.WriteLine(sb.ToString());
        }
    }
}


