using System.IO;
using System.Text;

namespace metrics.Reporting
{
    /// <summary>
    /// A file-based reporter that outputs formatted metrics to a single flat file
    /// </summary>
    public class FileReporter : ReporterBase
    {
        public FileReporter(string path, Metrics metrics) : base(new StreamWriter(path, true, Encoding.UTF8),metrics)
        {
            
        }

        public FileReporter(string path, Encoding encoding, Metrics metrics)
            : base(new StreamWriter(path, true, encoding), metrics)
        {
            
        }

        public FileReporter(string path, IReportFormatter formatter)
            : base(new StreamWriter(path, true, Encoding.UTF8), formatter)
        {

        }

        public FileReporter(string path, Encoding encoding, IReportFormatter formatter) : base(new StreamWriter(path, true, encoding), formatter)
        {

        }
    }
}
