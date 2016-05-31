using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;
using email.Providers;

namespace email.Delivery
{
    /// <summary>
    /// A service for efficiently delivering email, with graceful interruption, using a provided outgoing email service
    /// </summary>
    public class DeliveryService : IDeliveryService
    {
        private readonly IEmailProvider _provider;
        private readonly IDeliveryConfiguration _config;
        private readonly DeliveryObserver _observer;
        private readonly BlockingCollection<EmailMessage> _outgoing;
        private readonly ConcurrentQueue<EmailMessage> _backlog;
        private readonly CancellationTokenSource _cancel;
        private readonly JsonSerializer<EmailMessage> _serializer;

        private int _delivered;
        private readonly Stopwatch _uptime = new Stopwatch();

        private bool _started;
        private Task _background;

        private readonly string _backlogFolder;
        private readonly string _undeliverableFolder;

        public int Delivered
        {
            get { return _delivered; }
        }
        public TimeSpan Uptime
        {
            get { return _uptime.Elapsed; }   
        }
        public double DeliveryRate
        {
            get { return _delivered/_uptime.Elapsed.TotalSeconds; }
        }

        public DeliveryService(IEmailProvider provider, IDeliveryConfiguration config)
        {
            _provider = provider;
            _config = config;

            _outgoing = new BlockingCollection<EmailMessage>();
            _backlog = new ConcurrentQueue<EmailMessage>();
            _observer = new DeliveryObserver(this);
            _cancel = new CancellationTokenSource();

            _backlogFolder = _config.BacklogFolder ?? "backlog";
            _undeliverableFolder = config.UndeliverableFolder ?? "undeliverable";
            _serializer = new JsonSerializer<EmailMessage>();
            
            if (!Directory.Exists(_backlogFolder))
            {
                Directory.CreateDirectory(_backlogFolder);
            }
            if (!Directory.Exists(_undeliverableFolder))
            {
                Directory.CreateDirectory(_undeliverableFolder);
            }
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = _config.MaxDegreeOfParallelism.HasValue ? _config.MaxDegreeOfParallelism.Value : 10,
                CancellationToken = _cancel.Token
            };

            if(_background != null)
            {
                Stop();
                _background.Dispose();
                _background = null;
            }

            ReseedQueueFromBacklog();

            _background = Task.Factory.StartNew(() =>
            {
                try
                {
                    if(_config.MaxDeliveryRate.HasValue)
                    {
                        Parallel.ForEach(_outgoing.GetConsumingPartitioner(), options, (message, state) =>
                        {
                            while (DeliveryRate > _config.MaxDeliveryRate)
                            {
                                Thread.Sleep(10);
                            }
                            DeliveryCycle(options, message, state);
                        });
                    }
                    else
                    {
                        Parallel.ForEach(_outgoing.GetConsumingPartitioner(), options, (message, state) => DeliveryCycle(options, message, state));
                    }
                }
                catch (OperationCanceledException)
                {
                    while (!_outgoing.IsCompleted)
                    {
                        _backlog.Enqueue(_outgoing.Take());
                    }
                }
            });

            _uptime.Start();
            _started = true;
        }

        private void DeliveryCycle(ParallelOptions options, EmailMessage message, ParallelLoopState state)
        {
            if (state.ShouldExitCurrentIteration)
            {
                _backlog.Enqueue(message);
            }
            else
            {
                if(message.DeliveryTime.HasValue)
                {
                    if(message.DeliveryTime.Value.ToUniversalTime() > DateTime.UtcNow)
                    {
                        if(_outgoing.IsAddingCompleted)
                        {
                            _backlog.Enqueue(message);
                        }
                        else
                        {
                            _outgoing.Add(message);    
                        }
                        return;
                    }
                }

                if (_provider.Send(message))
                {
                    Interlocked.Increment(ref _delivered);
                }
                else
                {
                    // Back seat in the queue
                    if(_config.RetryPolicy != null)
                    {
                        var decision = _config.RetryPolicy.DecideOn(message);
                        switch(decision)
                        {
                            case DeliveryRetryDecision.RetryImmediately:
                                DeliveryCycle(options, message, state);
                                break;
                            case DeliveryRetryDecision.SendToBackOfQueue:
                                _outgoing.Add(message);
                                break;
                            case DeliveryRetryDecision.SendToBacklog:
                                _backlog.Enqueue(message);
                                break;
                            case DeliveryRetryDecision.SendToUndeliverableFolder:
                                Undeliverable(message);
                                break;
                            case DeliveryRetryDecision.Destroy:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        _outgoing.Add(message);
                    }
                }
            }

            options.CancellationToken.ThrowIfCancellationRequested();
        }

        private void ReseedQueueFromBacklog()
        {
            var files = Directory.GetFiles(_backlogFolder, "*.json");
            foreach (var file in files)
            {
                var json = File.ReadAllText(file, Encoding.UTF8);
                var message = _serializer.DeserializeFromString(json);
                _outgoing.Add(message);
            }
        }

        public void Send(EmailMessage message)
        {
            if(message == null)
            {
                return;
            }
            if (_outgoing.IsAddingCompleted || !_started)
            {
                _backlog.Enqueue(message);
            }
            else
            {
                _outgoing.Add(message);
            }
        }

        public void Send(IEnumerable<EmailMessage> messages)
        {
            if (messages == null)
            {
                return;
            }

            if(_outgoing.IsAddingCompleted)
            {
                foreach (var message in messages)
                {
                    _backlog.Enqueue(message);
                }
            }
            else
            {
                foreach (var message in messages)
                {
                    _outgoing.Add(message);
                }    
            }
        }

        public void Send(IObservable<EmailMessage> source)
        {
            source.Subscribe(_observer);
        }
        
        public void Stop()
        {
            Stop(DeliveryCancellationHandling.SendToBacklog);
        }

        public void Stop(DeliveryCancellationHandling handling)
        {
            if(!_started)
            {
                return;
            }
            
            _outgoing.CompleteAdding();

            switch(handling)
            {
                case DeliveryCancellationHandling.SendToBacklog:
                    _cancel.Cancel();
                    break;
            }

            while (!_outgoing.IsCompleted)
            {
                Thread.Sleep(100);
            }

            SaveBacklog();

            _started = false;
            _uptime.Stop();
        }

        private void SaveBacklog()
        {
            EmailMessage message;
            var toSave = new List<EmailMessage>();
            while(_backlog.TryDequeue(out message))
            {
                toSave.Add(message);
            }
            Parallel.ForEach(toSave, Backlog);
        }

        internal void Backlog(EmailMessage message)
        {
            var folder = _backlogFolder;
            var json = _serializer.SerializeToString(message);
            var path = Path.Combine(folder, string.Concat(message.Id, ".json"));
            File.WriteAllText(path, json);
        }

        private void Undeliverable(EmailMessage message)
        {
            var folder = _undeliverableFolder;
            var json = _serializer.SerializeToString(message);
            var path = Path.Combine(folder, string.Concat(message.Id, ".json"));
            File.WriteAllText(path, json);
        }

        public void Dispose()
        {
            if(_started)
            {
                Stop();
            }
            if(_backlog != null && _backlog.Count > 0)
            {
                SaveBacklog();
            }
        }
    }
}
