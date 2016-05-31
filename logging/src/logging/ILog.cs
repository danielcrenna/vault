using System;

namespace logging
{
    public interface ILog
    {
        bool TraceEnabled { get; }
        bool DebugEnabled { get; }
        bool InfoEnabled { get; }
        bool WarnEnabled { get; }
        bool ErrorEnabled { get; }
        bool FatalEnabled { get; }

        void Trace(Func<string> message);
        void Trace(Func<string> message, Exception exception);
        void Debug(Func<string> message);
        void Debug(Func<string> message, Exception exception);
        void Info(Func<string> message);
        void Info(Func<string> message, Exception exception);
        void Warn(Func<string> message);
        void Warn(Func<string> message, Exception exception);
        void Error(Func<string> message);
        void Error(Func<string> message, Exception exception);
        void Fatal(Func<string> message);
        void Fatal(Func<string> message, Exception exception);
    }
}
