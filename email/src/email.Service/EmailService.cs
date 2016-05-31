using System.ServiceProcess;
using email.Delivery;

namespace email.Service
{
    /// <summary>
    /// A Windows service that watches for serialized JSON messages dumped to a pickup directory, and delivers them
    /// with the configured <see cref="IEmailService"/> implementation. It is just a passthrough to 
    /// <see cref="DeliveryService"/>.
    /// <remarks>
    ///     - This is NOT an MTA; make sure the service on the other end is reliable!
    /// </remarks>
    /// </summary>
    public partial class EmailService : ServiceBase
    {
        private DeliveryService _service;

        public EmailService()
        {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            _service = ConfigurationRoot.GetInstance<DeliveryService>();
            _service.Start();
        }

        protected override void OnStop()
        {
            if(_service == null)
            {
                return;
            }
            _service.Stop(DeliveryCancellationHandling.SendToBacklog);
            _service.Dispose();
            _service = null;
        }
    }
}
