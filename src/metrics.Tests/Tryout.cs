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

            var docsTimedCounterPerSec = db1Metrics.TimedCounter("db1", "docs new indexed/sec", "new Indexed Documents");

            for (int i = 0; i < 1000; i++)
            {
                docsTimedCounterPerSec.Mark();
                Thread.Sleep(3);
            }
            Console.WriteLine(docsTimedCounterPerSec.LastValue);


        } 
    }
}