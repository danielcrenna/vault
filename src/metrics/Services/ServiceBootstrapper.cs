using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace metrics.Services
{
    public class ServiceBootstrapper
    {
        public static void Run<T>(ServiceDescriptor descriptor, params string[] args) where T : ServiceBase, new()
        {
            if (args.Length == 1 && args[0] == "/u")
            {
                descriptor.Uninstall();
                Console.WriteLine("{0} uninstalled.", descriptor.DisplayName);
                return;
            }

            if (args.Length == 1 && args[0] == "/r")
            {
                descriptor.Reinstall();
                Console.WriteLine("{0} reinstalled.", descriptor.DisplayName);
            }
            
            if (args.Length == 1 && args[0] == "/i")
            {
                descriptor.Install();
                Console.WriteLine("{0} installed.", descriptor.DisplayName);
            }

            var instance = new T();

            if (!Debugger.IsAttached)
            {
                ServiceBase.Run(new ServiceBase[] {instance});
            }
        }
    }
}
