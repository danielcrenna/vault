namespace logging.nlog
{
    public class NLogFactory : ILogFactory
    {
        public ILog GetLogger(string name)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(name));
        }

        public ILog GetLogger()
        {
            return new NLogLogger(NLog.LogManager.GetCurrentClassLogger());
        }
    }
}