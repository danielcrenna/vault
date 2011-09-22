using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace metrics.Services.Deployment
{
    public class ServiceBootstrapper
    {
        public static void Run<T>(ServiceDescriptor descriptor, params string[] args) where T : InstrumentedServiceBase, new()
        {
            if (args.Length == 1 && args[0] == "/u")
            {
                descriptor.Uninstall();
                Console.Out.WriteLine("{0} uninstalled.", descriptor.DisplayName);
                return;
            }

            if (args.Length == 1 && args[0] == "/r")
            {
                descriptor.Reinstall();
                Console.Out.WriteLine("{0} reinstalled.", descriptor.DisplayName);
            }
            
            if (args.Length == 1 && args[0] == "/i")
            {
                descriptor.Install();
                Console.Out.WriteLine("{0} installed.", descriptor.DisplayName);
            }

            var instance = new T();

            if (Debugger.IsAttached)
            {
                instance.ManualStart();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] {instance});
            }
        }
    }
}
