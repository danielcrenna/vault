using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;

namespace metrics.Services.Deployment
{
    /// <summary>
    /// A bootstrapper component that is responsible for installing and uninstalling a Windows Service
    /// <remarks>
    ///     - Based on source code extracted from TopShelf: https://github.com/phatboyg/Topshelf under Apache 2.0 license
    /// </remarks>
    /// </summary>
    public class WindowsServiceInstaller : Installer
    {
        private readonly string _name;
        private readonly string _description;
        private readonly string _displayName;

        public WindowsServiceInstaller(ServiceDescriptor descriptor)
        {
            _name = descriptor.Name;
            _description = descriptor.Description;
            _displayName = descriptor.DisplayName;
        }
        
        public override void Install(IDictionary stateSaver)
        {
            var installers = CreateServiceInstallersFromDescriptor();

            Installers.AddRange(installers.ToArray());

            try
            {
                base.Install(stateSaver);
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1073) // "The specified service already exists"
                {
                    Console.Out.WriteLine(ex.Message);
                }
                else
                {
                    throw;
                }
            }

            using (var system = Registry.LocalMachine.OpenSubKey("System"))
            {
                if (system == null)
                {
                    return;
                }

                using (var currentControlSet = system.OpenSubKey("CurrentControlSet"))
                {
                    if (currentControlSet == null)
                    {
                        return;
                    }

                    using (var services = currentControlSet.OpenSubKey("Services"))
                    {
                        if (services == null)
                        {
                            return;
                        }

                        using (var service = services.OpenSubKey(_name, true))
                        {
                            if (service == null)
                            {
                                return;
                            }

                            service.SetValue("Description", _description);

                            var imagePath = (string) service.GetValue("ImagePath");
                                            
                            service.SetValue("ImagePath", imagePath);
                        }
                    }
                }
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            var installers = CreateServiceInstallersFromDescriptor();

            Installers.AddRange(installers.ToArray());

            var state = new Hashtable { { "ServiceName", _name }, { "Description", _description }, { "DisplayName", _displayName } };

            base.Uninstall(state);
        }

        private IEnumerable<Installer> CreateServiceInstallersFromDescriptor()
        {
            yield return new ServiceInstaller
            {
                ServiceName = _name,
                Description = _description,
                DisplayName = _displayName,
                StartType = ServiceStartMode.Automatic
            };

            yield return new ServiceProcessInstaller
            {
                Username = "", 
                Password = "",
                Account = ServiceAccount.LocalSystem
            };
        }
    }
}