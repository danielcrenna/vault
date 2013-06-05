using System;
using System.Messaging;

namespace copper
{
    /// <summary>
    /// A consumer that enqueues with MSMQ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueueConsumer<T> : AsyncConsumer<T>
    {
        private readonly Lazy<MessageQueue> _queue;

        public MessageQueue Queue
        {
            get { return _queue.Value; }
        }
        
        public MessageQueueConsumer(string queueName)
        {
            _queue = new Lazy<MessageQueue>(() => MessageQueue.Exists(queueName) ? new MessageQueue(queueName) : MessageQueue.Create(queueName));
        }

        public override bool Handle(T @event)
        {
            try
            {
                var message = new Message(@event);
                _queue.Value.Send(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}