using System.ServiceProcess;

namespace email.Service
{
    static class Program
    {
        static void Main()
        {
            var servicesToRun = new ServiceBase[] { new EmailService() };
            ServiceBase.Run(servicesToRun);
        }
    }
}
