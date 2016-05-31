using System.Configuration;
using Munq;
using email.Postmark;
using email.Providers;
using email.Templates;

namespace email.Service
{
    public class ConfigurationRoot
    {
        private static readonly IocContainer Container;

        static ConfigurationRoot()
        {
            Container = new IocContainer();
            Container.Register<IEmailProvider>(r => new PostmarkEmailProvider(ConfigurationManager.AppSettings["PostmarkServerToken"]));
            Container.Register<IEmailTemplateEngine>(r => new DotLiquidEmailTemplateEngine());
        }

        public static T GetInstance<T>() where T : class
        {
            return Container.Resolve<T>();
        }
    }
}