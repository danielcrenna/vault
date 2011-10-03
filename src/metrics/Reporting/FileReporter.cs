using metrics.Core;

namespace metrics.Reporting
{
    public class FileReporter : ReporterBase
    {
        public FileReporter(string path)
            : base(new System.IO.StreamWriter(path, true, System.Text.Encoding.UTF8))
        { }

        public FileReporter(string path, System.Text.Encoding encoding) 
            : base (new System.IO.StreamWriter(path, true, encoding))
        { }
    }
}
