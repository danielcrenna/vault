using container;

namespace logging
{
    public static class LogManager
    {
        private static readonly Container Container;
        static LogManager()
        {
            Container = new Container();
        }

        public static void Install<T>() where T : ILogFactory, new()
        {
            Container.Remove<ILogFactory>();
            Container.Register<ILogFactory>(r => new T());
        }

        public static ILog GetLogger(string name)
        {
            return Container.Resolve<ILogFactory>().GetLogger(name);
        }

        public static ILog GetLogger()
        {
            return Container.Resolve<ILogFactory>().GetLogger();
        }
    }
}