using System;
using System.IO;
using System.Timers;
using metrics.Core;
using metrics.Serialization;

namespace metrics.Reporting
{
    public class JsonReporter : ReporterBase
    {
        private readonly Timer _timer;
        private readonly string _path;

        public JsonReporter(string path)
            : this(path, 1)
        { }

        public JsonReporter(string path, double interval)
            : base(TextWriter.Null)
        {
            _path = path;
            _timer = new Timer
                         {
                             AutoReset = true,
                             Interval = TimeSpan.FromSeconds(interval).Ticks
                         };
            _timer.Elapsed += Elapsed;
        }

        public override void Run()
        {
            _timer.Start();
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            // Ensure base.Out is closed safely.
            if (Out != null) Out.Close();

            using (Out = new StreamWriter(GenerateFilePath()))
            {
                Out.Write(Serializer.Serialize(Metrics.All));
            }
        }

        private string GenerateFilePath()
        {
            return Path.Combine(_path, DateTime.Now.Ticks.ToString());
        }
    }
}
