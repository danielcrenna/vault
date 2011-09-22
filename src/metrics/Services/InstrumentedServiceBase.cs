using System.ServiceProcess;
using metrics.Net;

namespace metrics.Services
{
    public class InstrumentedServiceBase : ServiceBase
    {
        private readonly MetricsListener _listener;

        public InstrumentedServiceBase()
        {
            MachineMetrics.InstallAll();

            _listener = new MetricsListener();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            ManualStart();
        }
         
        protected override void OnStop()
        {
            base.OnStop();

            ManualStop();
        }

        public virtual void ManualStop()
        {
            _listener.Stop();
        }

        public virtual void ManualStart()
        {
            _listener.Start(9595);
        }
    }
}
