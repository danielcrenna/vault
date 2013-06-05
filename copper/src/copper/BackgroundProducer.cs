using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using copper.Extensions;

namespace copper
{
    /// <summary>
    /// A default producer implementation that produces on a background thread
    /// <remarks>
    /// - The production queue is seeded explicitly by callers, or by subscribing to an observable
    /// - The producer-consumer problem requires a shared buffer; you must give full control of the buffer to the producer, and pass it to any consumers
    /// - Backlogged and undeliverable events are managed by other consumers you can attach; by default, all special case event handling is sent into the abyss
    /// </remarks> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BackgroundProducer<T> : Produces<T>, IDisposable
    {
        private bool _started;
        private int _sent;
        private int _undelivered;
        
        private Task _background;
        private CancellationTokenSource _cancel;
        private readonly SemaphoreSlim _empty;
        private readonly Stopwatch _uptime;

        private Consumes<T> _consumer;
        private Consumes<T> _backlogConsumer;
        private Consumes<T> _undeliverableConsumer;
        
        public int MaxDegreeOfParallelism { get; set; }
        
        public TimeSpan Uptime
        {
            get { return _uptime.Elapsed; }
        }

        public int Sent
        {
            get { return _sent; }
        }

        public double Rate
        {
            get { return _sent / _uptime.Elapsed.TotalSeconds; }
        }

        public int Queued
        {
            get { return Buffer.Count; }
        }
        
        public int Undeliverable
        {
            get { return _undelivered; }
        }

        public RetryPolicy RetryPolicy { get; private set; }

        public RateLimitPolicy RateLimitPolicy { get; private set; }

        public BlockingCollection<T> Buffer { get; private set; }

        public BackgroundProducer(IObservable<T> source) : this()
        {
            Produce(source);
        }

        public BackgroundProducer(): this(new BlockingCollection<T>())
        {

        }

        public BackgroundProducer(int capacity) : this(new BlockingCollection<T>(capacity))
        {

        }

        public BackgroundProducer(IProducerConsumerCollection<T> source) : this(new BlockingCollection<T>(source))
        {
            
        }

        public BackgroundProducer(IProducerConsumerCollection<T> source, int capacity) : this(new BlockingCollection<T>(source, capacity))
        {

        }

        public BackgroundProducer(BlockingCollection<T> source)
        {
            Buffer = source;
            MaxDegreeOfParallelism = 1;
            _uptime = new Stopwatch();
            _cancel = new CancellationTokenSource();
            _empty = new SemaphoreSlim(1);
            RetryPolicy = new RetryPolicy();
            RateLimitPolicy = new RateLimitPolicy();
            _consumer = new CollectionConsumer<T>();
        }

        public void Produce(T @event)
        {
            if (Buffer.IsAddingCompleted)
            {
                // If we added to the backlog queue while stopping, then observables could fill it after a flush 
                HandleBacklog(@event);
            }
            else
            {
                Buffer.Add(@event);
            }
        }

        public void Produce(IList<T> events)
        {
            if (events.Count == 0) return;
            foreach(var @event in events)
            {
                Produce(@event);
            }
        }
        
        public void Produce(IObservable<T> observable)
        {
            if(Buffer.IsAddingCompleted)
            {
                throw new InvalidOperationException("You cannot subscribe the buffer while stopping");
            }
            observable.Subscribe(Produce, exception => { }, () => { }, _cancel.Token);
        }

        public void Produce(IObservable<IList<T>> observable)
        {
            if (Buffer.IsAddingCompleted)
            {
                throw new InvalidOperationException("You cannot subscribe the buffer while stopping");
            }
            observable.Subscribe(Produce, exception => { }, () => { }, _cancel.Token);
        }
        
        public void Attach(Consumes<T> consumer)
        {
            _consumer = consumer;
        }

        public void AttachBacklog(Consumes<T> consumer)
        {
            _backlogConsumer = consumer;
        }

        public void AttachBacklog(Action<T> @delegate)
        {
            _backlogConsumer = new DelegatingConsumer<T>(@delegate);
        }

        public void AttachUndeliverable(Consumes<T> consumer)
        {
            _undeliverableConsumer = consumer;
        }

        public void AttachUndeliverable(Action<T> @delegate)
        {
            _undeliverableConsumer = new DelegatingConsumer<T>(@delegate);
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            if (_background != null)
            {
                Stop();
                _background.Dispose();
                _background = null;
            }

            ConfigureStartSettings();

            RequisitionBackgroundTask();

            _uptime.Start();
            _started = true;
        }

        private void ConfigureStartSettings()
        {
            var devNull = new DelegatingConsumer<T>(@event => { });
            _backlogConsumer = devNull;
            _undeliverableConsumer = devNull;
        }

        public void Stop()
        {
            if (!_started)
            {
                return;
            }

            Buffer.CompleteAdding();

            WaitForEmptyBuffer();

            ResetToInitialState();
        }

        private void WaitForEmptyBuffer()
        {
            _cancel.Cancel();
            _cancel.Token.WaitHandle.WaitOne();
            WithEmptyWait(() => { /* Wait for cancellation to empty the buffer to the backlogging consumer */ });
        }

        private void ResetToInitialState()
        {
            _started = false;
            Buffer = new BlockingCollection<T>();
        }

        private async void HandleBacklog(T @event)
        {
            if (!await _backlogConsumer.HandleAsync(@event))
            {
                HandleUndeliverable(@event);
            }
        }

        private async void HandleUndeliverable(T @event)
        {
            await Task.Factory.StartNew(() =>
            {
                _undeliverableConsumer.Handle(@event);
                Interlocked.Increment(ref _undelivered);
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_started)
            {
                Stop();
            }
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }      
            if(_cancel != null)
            {
                _cancel.Dispose();
                _cancel = null;
            }
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void RequisitionBackgroundTask()
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = _cancel.Token
            };
           
            _background = Task.Factory.StartNew(() =>
            {
                try
                {
                    ProduceOn(GetProductionSource(), options);
                }
                catch (OperationCanceledException)
                {
                    TransferBufferToBacklog();
                }
            });
        }

        private BlockingCollection<T> GetProductionSource()
        {
            BlockingCollection<T> source;
            if (RateLimitPolicy.Enabled)
            {
                // Convert the outgoing blocking collection into a rate limited observable, then feed a new blocking queue with it
                var sequence = Buffer.AsRateLimitedObservable(RateLimitPolicy.Occurrences, RateLimitPolicy.TimeUnit, _cancel.Token);
                source = new BlockingCollection<T>();
                sequence.Subscribe(source.Add, exception => { }, () => { });
            }
            else
            {
                source = Buffer;
            }
            return source;
        }

        private void TransferBufferToBacklog()
        {
            _empty.Wait();
            while (!Buffer.IsCompleted)
            {
                Backlog(Buffer.Take());
            }
            _empty.Release();
        }
        
        private void ProduceOn(BlockingCollection<T> source, ParallelOptions options)
        {
            var partitioner = source.GetConsumingPartitioner();
            Parallel.ForEach(partitioner, options, (@event, state) => ProductionCycle(options, @event, state));
        }

        private readonly IDictionary<int, int> _attempts = new ConcurrentDictionary<int, int>(); 
        private void ProductionCycle(ParallelOptions options, T @event, ParallelLoopState state)
        {
            if (state.ShouldExitCurrentIteration)
            {
                Backlog(@event);
                return;
            }

            if (!_consumer.Handle(@event))
            {
                HandleUnsuccessfulDelivery(options, @event, state);
            }
            
            Interlocked.Increment(ref _sent);
            options.CancellationToken.ThrowIfCancellationRequested();
        }

        private void Backlog(T @event)
        {
            _backlogConsumer.Handle(@event);
        }

        private void HandleUnsuccessfulDelivery(ParallelOptions options, T @event, ParallelLoopState state)
        {
            if (RetryPolicy != null)
            {
                var decision = RetryPolicy.DecideOn(@event,  GetAttempts(@event));

                switch (decision)
                {
                    case RetryDecision.RetryImmediately:
                        ProductionCycle(options, @event, state);
                        break;
                    case RetryDecision.Requeue:
                        Buffer.Add(@event);
                        break;
                    case RetryDecision.Backlog:
                        Backlog(@event);
                        break;
                    case RetryDecision.Undeliverable:
                        HandleUndeliverable(@event);
                        break;
                    case RetryDecision.Destroy:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                Buffer.Add(@event);
            }
        }

        private int GetAttempts(T @event)
        {
            int attempts;
            var hash = @event.GetHashCode();
            if (!_attempts.TryGetValue(hash, out attempts))
            {
                _attempts.Add(hash, 1);
            }
            else
            {
                _attempts[hash] = _attempts[hash] + 1;
            }
            attempts = _attempts[hash];
            return attempts;
        }
        
        private void WithEmptyWait(Action closure)
        {
            _empty.Wait();
            closure();
            _empty.Release();
        }
    }
}