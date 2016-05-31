namespace email.Delivery
{
    public interface IDeliveryService : IEmailService
    {
        void Start();
        void Stop();
        void Stop(DeliveryCancellationHandling handling);
    }
}