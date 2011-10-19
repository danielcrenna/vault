using System;
using System.IO;
using System.Text;
using metrics.Serialization;

namespace metrics.Reporting
{
    /// <summary>
    /// A file-based reporter that produces a timestamped output file for each sample collection
    /// </summary>
    public class SampledFileReporter : ReporterBase
    {
        private readonly Encoding _encoding;
        private readonly string _path;

        public SampledFileReporter(string path) : this(path, Encoding.UTF8, new HumanReadableReportFormatter())
        {
            
        }

        public SampledFileReporter(string path, Encoding encoding) : this(path, encoding, new HumanReadableReportFormatter())
        {
            
        }

        public SampledFileReporter(string path, Encoding encoding, IReportFormatter formatter) : base (null, formatter)
        {
            _path = path;
            _encoding = encoding;
        }
        
        public override void Run()
        {
            using (Out = new StreamWriter(GenerateFilePath(), false, _encoding))
            {
                Out.Write(Serializer.Serialize(Metrics.All));
                Out.Flush();
            }
        }

        private string GenerateFilePath()
        {
            return Path.Combine(_path, string.Format("{0}.json", DateTime.Now.Ticks));
        }
    }
}