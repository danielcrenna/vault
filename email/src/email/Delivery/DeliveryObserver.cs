using System;

namespace email.Delivery
{
    /// <summary>
    /// A little thing that queues a <see cref="DeliveryService"/> when an <see cref="EmailMessage"/> is observed
    /// </summary>
    public class DeliveryObserver : IObserver<EmailMessage>
    {
        private readonly DeliveryService _parent;

        public DeliveryObserver(DeliveryService parent)
        {
            _parent = parent;
        }

        public void OnNext(EmailMessage value)
        {
            _parent.Send(value);
        }

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {

        }
    }
}