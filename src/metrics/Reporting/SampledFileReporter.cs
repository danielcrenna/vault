using System;
using System.IO;
using System.Text;
using metrics.Serialization;

namespace metrics.Reporting
{
    /// <summary>
    /// A file-based reporter that produces a timestamped-suffixed output file for each sample collection
    /// </summary>
    public class SampledFileReporter : ReporterBase
    {
        private readonly Encoding _encoding;
        private readonly string _directory;

        public SampledFileReporter() : this("", Encoding.UTF8, new HumanReadableReportFormatter())
        {

        }

        public SampledFileReporter(Encoding encoding) : this("", encoding, new HumanReadableReportFormatter())
        {

        }

        public SampledFileReporter(IReportFormatter formatter) : this("", Encoding.UTF8, formatter)
        {

        }

        public SampledFileReporter(string directory) : this(directory, Encoding.UTF8, new HumanReadableReportFormatter())
        {
            
        }

        public SampledFileReporter(string directory, Encoding encoding) : this(directory, encoding, new HumanReadableReportFormatter())
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
                var metrics = new Metrics();
 
                Out.Write(Serializer.Serialize(metrics.AllSorted));
                Out.Flush();
            }
        }

        private string GenerateFilePath()
        {
            return Path.Combine(_directory, string.Format("{0}.sample", DateTime.Now.Ticks));
        }
    }
}