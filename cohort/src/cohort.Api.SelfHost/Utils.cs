using System;
using System.Diagnostics;

namespace cohort.Api.SelfHost
{
    public class Utils
    {
        public static void Execute(string filename, string arguments, bool trace = false)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    Verb = "runas"
                }
            };
            p.Start();
            p.WaitForExit(30000);
            if (!trace) return;
            var output = p.StandardOutput.ReadToEnd();
            Console.Write(output);
        }
    }
}
