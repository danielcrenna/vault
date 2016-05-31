using System;

namespace Hammock.Tasks
{
    public interface ITaskState
    {
        int RepeatCount { get; set; }
        DateTime? LastRepeat { get; set; }
    }

    public interface IRetryState :ITaskState
    {
    }
}
