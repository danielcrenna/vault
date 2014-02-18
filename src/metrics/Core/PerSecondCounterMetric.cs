using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using metrics.Stats;
using metrics.Support;
using System.Text;
using System.Runtime.Serialization;

namespace metrics.Core
{
    public class PerSecondCounterMetric : IMetric, IDisposable
    {
        private readonly string _eventType;
        private readonly TimeUnit _rateUnit;
        readonly AtomicLong _value = new AtomicLong();
        private AtomicLong _prev = new AtomicLong();
        private readonly CancellationTokenSource _token = new CancellationTokenSource();

        private readonly ConcurrentQueue<long> _history = new ConcurrentQueue<long>();

        private const int HistorySize = 60 * 5;

        private void TimeElapsed()
        {
            _prev.Set(_value.Get());
            _history.Enqueue(_prev.Get());
            if (_history.Count > HistorySize)
            {
                long _;
                _history.TryDequeue(out _);
            }
            _value.Set(0);
        }
        public void LogJson(StringBuilder sb)
        {
            sb.Append("{\"count\":").Append(LastValue)
              .Append(",\"rate unit\":").Append(RateUnit).Append("}");

        }
        [IgnoreDataMember]
        public IMetric Copy
        {
            get
            {
                var metric = new PerSecondCounterMetric(EventType, RateUnit);

                return metric;
            }
        }

        public void Mark()
        {
            _value.IncrementAndGet();
        }

        public IEnumerable<double> Values()
        {
            var currentValue = CurrentValue;
            foreach (var l in _history)
            {
                yield return l;
            }
            yield return currentValue;

        }

        public double CurrentValue
        {
            get { return _prev.Get(); }
        }

        public string EventType
        {
            get { return _eventType; }
        }

        public TimeUnit RateUnit
        {
            get { return _rateUnit; }
        }

        private PerSecondCounterMetric(string eventType, TimeUnit rateUnit)
        {
            _eventType = eventType;
            _rateUnit = rateUnit;
        }

        public static PerSecondCounterMetric New(string eventType)
        {
            var meter = new PerSecondCounterMetric(eventType, TimeUnit.Seconds);

            var interval = TimeSpan.FromSeconds(1);

            Task.Factory.StartNew(async () =>
            {
                while (!meter._token.IsCancellationRequested)
                {
                    await Task.Delay(interval, meter._token.Token);
                    meter.TimeElapsed();
                }
            }, meter._token.Token);

            return meter;
        }

        public void Dispose()
        {
            _token.Cancel();
        }
    }
}