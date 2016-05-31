//using email.Delivery;
//using linger;

//namespace email.Linger
//{
//    /// <summary>
//    /// Allows delivery to run as a hosted job in linger.
//    /// </summary>
//    public class DeliveryServiceHost : IHostedJob
//    {
//        private readonly IDeliveryService _service;
//        public IDeliveryService Service { get { return _service; } }
//        public DeliveryServiceHost(IDeliveryService service)
//        {
//            _service = service;
//        }
//        public void Perform()
//        {
//            _service.Start();
//        }
//        public void Halt(bool immediate)
//        {
//            var handling = immediate ? DeliveryCancellationHandling.SendToBacklog : DeliveryCancellationHandling.EmptyQueue;
//            _service.Stop(handling);
//        }
//    }
//}
