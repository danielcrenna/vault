using System;
using System.IO;
using System.Threading;
using metrics.Core;
using metrics.Util;

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
