using System.Threading;
using NUnit.Framework;

namespace metrics.Tests.Core
{
   [TestFixture]
   public class TimerTests : MetricTestBase
   {
 
      [Test]
      public void CallbackTimerTestBasic()
      {

          var metrics = new Metrics();
         var timer = metrics.CallbackTimer(typeof(TimerTests), "CallbackTimertest", TimeUnit.Milliseconds, TimeUnit.Milliseconds);

         for (int i = 0; i < 10; i++)
         {
            var ctx = timer.Time();
            Thread.Sleep(250);
            ctx.Stop();
         }

         Assert.AreEqual(250d, timer.Mean, 5); // made-up delta values that "seems" right
         Assert.AreEqual(1d, timer.StdDev, 1);
      }

      [Test]
      public void ManualTimerTestBasic()
      {
          var metrics = new Metrics();
 
         var timer = metrics.ManualTimer(typeof(TimerTests), "|ManualTimertest", TimeUnit.Milliseconds, TimeUnit.Milliseconds);

         for (int i = 0; i < 10; i++)
         {
            timer.RecordElapsedMillis(250);
         }

         Assert.AreEqual(250d, timer.Mean, 5); // made-up delta values that "seems" right
         Assert.AreEqual(1d, timer.StdDev, 1);
      }


   }
}