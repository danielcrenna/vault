using System;
using System.IO;
using System.Threading;
using metrics.Util;

namespace metrics.Reporting
{
    /// <summary>
    ///  A reporter that periodically prints out formatted application metrics to a specified <see cref="TextWriter" />
    /// </summary>
    public abstract class ReporterBase : IReporter
    {
        protected TextWriter Out;
        private readonly IReportFormatter _formatter;
        protected CancellationTokenSource Token;
        internal int Runs { get; set; } 

        protected ReporterBase(TextWriter writer) : this(writer, new HumanReadableReportFormatter())
        {
            Out = writer;
        }

        protected ReporterBase(TextWriter writer, IReportFormatter formatter)
        {
            Out = writer;
            _formatter = formatter;
        }

        /// <summary>
        /// Starts the reporting task for periodic output
        /// </summary>
        /// <param name="period">The period between successive displays</param>
        /// <param name="unit">The period time unit</param>
        public virtual void Start(long period, TimeUnit unit)
        {
            var seconds = unit.Convert(period, TimeUnit.Seconds);
            var interval = TimeSpan.FromSeconds(seconds);

            Token = Utils.StartCancellableTask(() =>
            {
                while (!Token.IsCancellationRequested)
                {
                    Thread.Sleep(interval);
                    Run();
                }
            });
        }

        public virtual void Run()
        {
            try
            {
                Out.Write(_formatter.GetSample());
                
                Out.Flush();

                Runs++;
            }
            catch (Exception e)
            {
                Out.WriteLine(e.StackTrace);
            }
        }

        public void Dispose()
        {
            if (Token != null)
            {
                Token.Cancel();
            }

            if (Out != null)
            {
                Out.Close();
            }
        }
    }
}
