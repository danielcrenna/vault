using System;
using System.Threading;
using metrics.Core;

namespace metrics.Tests
{
    public class Tryout
    {
        static void Main(string[] args)
        {
            var db1Metrics = new Metrics();

            var docsIndexedPerSec = db1Metrics.Meter("db1", "docs indexed/sec", "Indexed Documents", TimeUnit.Seconds);

            for (int i = 0; i < 200; i++)
            {
                docsIndexedPerSec.Mark();
                Thread.Sleep(500);
            }

            Console.WriteLine(docsIndexedPerSec.RateUnit);
            Console.WriteLine(docsIndexedPerSec.MeanRate);
            Console.WriteLine(docsIndexedPerSec.OneMinuteRate);
            Console.WriteLine(docsIndexedPerSec.FiveMinuteRate);
            Console.WriteLine(docsIndexedPerSec.FifteenMinuteRate);
            Console.WriteLine(docsIndexedPerSec.Count);
        } 
    }
}