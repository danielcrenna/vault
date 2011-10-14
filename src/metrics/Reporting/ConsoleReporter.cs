using System;
using System.IO;
using metrics.Core;

namespace metrics.Reporting
{
    /// <summary>
    ///  A simple reporters which prints out application metrics to a <see cref="TextWriter" /> periodically
    /// </summary>
    public class ConsoleReporter : ReporterBase
    {
        public ConsoleReporter()
            : base(Console.Out) { }
    }
}
