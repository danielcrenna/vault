using NLog;

namespace cohort.Logging
{
    public static class Logger
    {
        private static readonly NLog.Logger Log;
        static Logger()
        {
            Log = LogManager.GetLogger("cohort");
        }
        public static void Trace(string message, params object[] args)
        {
            Log.Trace(message, args);
        }
        public static void Debug(string message, params object[] args)
        {
            Log.Debug(message, args);
        }
        public static void Info(string message, params object[] args)
        {
            Log.Info(message, args);
        }
        public static void Warn(string message, params object[] args)
        {
            Log.Warn(message, args);
        }
        public static void Error(string message, params object[] args)
        {
            Log.Error(message, args);
        }
        public static void Fatal(string message, params object[] args)
        {
            Log.Fatal(message, args);
        }
    }
}
