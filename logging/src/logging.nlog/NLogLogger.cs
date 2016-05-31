using System;
using NLog;

namespace logging.nlog
{
    public class NLogLogger : ILog
    {
        private readonly Logger _inner;

        public NLogLogger(Logger inner)
        {
            _inner = inner;
        }

        public bool TraceEnabled { get { return _inner.IsTraceEnabled; } }
        public bool DebugEnabled { get { return _inner.IsDebugEnabled; } }
        public bool InfoEnabled { get { return _inner.IsInfoEnabled; } }
        public bool WarnEnabled { get { return _inner.IsWarnEnabled; } }
        public bool ErrorEnabled { get { return _inner.IsErrorEnabled; } }
        public bool FatalEnabled { get { return _inner.IsFatalEnabled; } }

        public void Trace(Func<string> message)
        {
            if (_inner.IsTraceEnabled)
            {
                _inner.Trace(message());
            }
        }

        public void Trace(Func<string> message, Exception exception)
        {
            if (_inner.IsTraceEnabled)
            {
                _inner.TraceException(message(), exception);
            }
        }

        public void Debug(Func<string> message)
        {
            if (_inner.IsDebugEnabled)
            {
                _inner.Debug(message());
            }
        }

        public void Debug(Func<string> message, Exception exception)
        {
            if (_inner.IsDebugEnabled)
            {
                _inner.DebugException(message(), exception);
            }
        }

        public void Info(Func<string> message)
        {
            if (_inner.IsInfoEnabled)
            {
                _inner.Info(message());
            }
        }

        public void Info(Func<string> message, Exception exception)
        {
            if (_inner.IsInfoEnabled)
            {
                _inner.InfoException(message(), exception);
            }
        }

        public void Warn(Func<string> message)
        {
            if (_inner.IsWarnEnabled)
            {
                _inner.Warn(message());
            }
        }

        public void Warn(Func<string> message, Exception exception)
        {
            if (_inner.IsWarnEnabled)
            {
                _inner.WarnException(message(), exception);
            }
        }

        public void Error(Func<string> message)
        {
            if (_inner.IsErrorEnabled)
            {
                _inner.Error(message());
            }
        }

        public void Error(Func<string> message, Exception exception)
        {
            if (_inner.IsErrorEnabled)
            {
                _inner.ErrorException(message(), exception);
            }
        }

        public void Fatal(Func<string> message)
        {
            if (_inner.IsFatalEnabled)
            {
                _inner.Fatal(message());
            }
        }

        public void Fatal(Func<string> message, Exception exception)
        {
            if (_inner.IsFatalEnabled)
            {
                _inner.FatalException(message(), exception);
            }
        }
    }
}
