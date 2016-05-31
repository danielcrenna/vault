using System;

namespace logging
{
    public interface ILogFactory
    {
        ILog GetLogger(string name);
        ILog GetLogger();
    }
}