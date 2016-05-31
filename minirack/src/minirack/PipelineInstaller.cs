using System;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using minirack;
using minirack.Extensions;

[assembly: WebActivator.PreApplicationStartMethod(typeof(PipelineInstaller), "Install")]

namespace minirack
{
    /// <summary>
    /// A "mini-rack", it acts as a plug for middleware that can intercept and change requests and responses at runtime.
    /// </summary>
    public class PipelineInstaller
    {
        public static void Install()
        {
            var disableValue = ConfigurationManager.AppSettings["minirack_Bypass"];
            bool disable;
            bool.TryParse(disableValue, out disable);
            if (disable) return;
            if(AppHarbor.IsHosting())
            {
                DynamicModuleUtility.RegisterModule(typeof(AppHarborModule));    
            }
            RegisterPipelineModules();
        }

        private static void RegisterPipelineModules()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(assembly => assembly.GetTypes());
            var modules = types.Where(t => t.HasAttribute<PipelineAttribute>() && t.Implements<IHttpModule>());
            var moduleOrder = modules.Select(m => new {m, i = m.GetAttribute<PipelineAttribute>().Order}).OrderBy(o => o.i);
            foreach (var order in moduleOrder)
            {
                DynamicModuleUtility.RegisterModule(order.m);
            }
        }
    }
}