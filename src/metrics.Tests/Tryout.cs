using System;
using System.Text;
using System.Threading;
using metrics.Core;

namespace metrics.Tests
{
    public class Tryout
    {
        static void Main(string[] args)
        {
            var db1Metrics = new Metrics();

            //var docsTimedCounterPerSec = db1Metrics.TimedCounter("db1", "docs new indexed/sec", "new Indexed Documents");

            //for (int i = 0; i < 100; i++)
            //{
            //    docsTimedCounterPerSec.Mark();
            //    Thread.Sleep(10);
            //}
            //Console.WriteLine(docsTimedCounterPerSec.CurrentValue);
         
            var RequestsPerSecondHistogram = db1Metrics.Histogram("db1", "Request Per Second Histogram");
            var RequestsPerSecondCounter = db1Metrics.TimedCounter("db1", "Request Per Second Counter","Request");
            for (int i = 0; i < 100; i++)
            {
                RequestsPerSecondCounter.Mark();
                RequestsPerSecondHistogram.Update((long)RequestsPerSecondCounter.CurrentValue);
                Thread.Sleep(10);
            }
            StringBuilder sb = new StringBuilder();
            double[] res;
            var perc = RequestsPerSecondHistogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);
            res = perc;
            RequestsPerSecondHistogram.LogJson(sb,perc);
            Console.WriteLine(sb);
            Console.WriteLine(RequestsPerSecondHistogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999));
           // RequestsPerSecondHistogram.Update((long)documentDatabase.WorkContext.MetricsCounters.RequestsPerSecondCounter.CurrentValue); //??

        } 
    }
}