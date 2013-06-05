using System;
using System.IO;

namespace metrics.Reporting
{
    
    public class ConsoleReporter : ReporterBase
    {
        public ConsoleReporter() : base(Console.Out)
        {
            
        }

        public ConsoleReporter(IReportFormatter formatter) : base(Console.Out, formatter)
        {

        }
    }
}
