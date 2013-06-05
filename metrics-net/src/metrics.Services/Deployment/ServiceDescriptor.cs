using System;
using System.Collections;
using System.Configuration.Install;
using System.IO;
using System.Reflection;

namespace metrics.Services.Deployment
{
    /// <summary>
    /// Functions as a description and an installation trigger for a Windows service
    /// <remarks>
    ///     - Based on source code extracted from TopShelf: https://github.com/phatboyg/Topshelf under Apache 2.0 license
    /// </remarks>
    /// </summary>
    public sealed class ServiceDescriptor
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }

        public void Install()
        {
            InstallWith(new WindowsServiceInstaller(this));
        }

        public void Uninstall()
        {
            UninstallWith(new WindowsServiceInstaller(this));
        }

        public void Reinstall()
        {
            Uninstall();

            Install();
        }

        public void InstallWith(WindowsServiceInstaller installer)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            
            using (var ti = new TransactedInstaller())
            {
                ti.Installers.Add(installer);

                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                {
                    throw new NullReferenceException("assembly");
                }

                var path = string.Format("/assemblypath={0}", assembly.Location);
                string[] commandLine = {path};

                var context = new InstallContext(null, commandLine);
                ti.Context = context;
                ti.Install(new Hashtable());
            }
        }

        public void UninstallWith(WindowsServiceInstaller windowsServiceInstaller)
        {
            windowsServiceInstaller.Uninstall(new Hashtable());
        }
    }
}
