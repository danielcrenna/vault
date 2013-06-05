using System;
using System.Messaging;
using System.Reactive.Linq;
using copper.Extensions;

namespace copper
{
    /// <summary>
    /// A producer that feeds its consumer with messages from MSMQ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueueProducer<T> : UsesBackgroundProducer<T> where T : class
    {
        private readonly Lazy<MessageQueue> _queue;
        
        public MessageQueueProducer(string queueName)
        {
            _queue = new Lazy<MessageQueue>(() => MessageQueue.Exists(queueName) ? new MessageQueue(queueName) : MessageQueue.Create(queueName));
            Background.Produce(new Func<T>(() =>
            {
                var message = _queue.Value.Receive();
                if (message == null) return null;
                return (T)message.Body;
            })
            .AsContinuousObservable()
            .Where(e => e != null)
            .Buffer(TimeSpan.FromMilliseconds(50)));
        }
    }
}