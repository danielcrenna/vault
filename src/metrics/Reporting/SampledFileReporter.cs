using System;
using System.IO;
using System.Text;

namespace metrics.Reporting
{
    /// <summary>
    /// A file-based reporter that produces a timestamped-suffixed output file for each sample collection
    /// </summary>
    public class SampledFileReporter : ReporterBase
    {
        private readonly Encoding _encoding;
        private readonly string _directory;

        public SampledFileReporter(Metrics metrics)
            : this("", Encoding.UTF8, new HumanReadableReportFormatter(metrics))
        {

        }

        public SampledFileReporter(Encoding encoding, Metrics metrics)
            : this("", encoding, new HumanReadableReportFormatter(metrics))
        {

        }

        public SampledFileReporter(IReportFormatter formatter) : this("", Encoding.UTF8, formatter)
        {

        }

        public SampledFileReporter(string directory, Metrics metrics)
            : this(directory, Encoding.UTF8, new HumanReadableReportFormatter(metrics))
        {
            
        }

        public SampledFileReporter(string directory, Encoding encoding, Metrics metrics)
            : this(directory, encoding, new HumanReadableReportFormatter(metrics))
        {
            
        }

        public SampledFileReporter(string directory, IReportFormatter formatter) : this(directory, Encoding.UTF8, formatter)
        {

        }

        public SampledFileReporter(string directory, Encoding encoding, IReportFormatter formatter) : base (null, formatter)
        {
            _directory = directory;
            _encoding = encoding;
        }
        
        public override void Run()
        {
            using (Out = new StreamWriter(GenerateFilePath(), false, _encoding))
            {
                Out.Write(_formatter.GetSample());
                Out.Flush();
            }
        }

        private string GenerateFilePath()
        {
            return Path.Combine(_directory, string.Format("{0}.sample", DateTime.Now.Ticks));
        }
    }
}