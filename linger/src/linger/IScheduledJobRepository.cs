using System.Collections.Generic;

namespace linger
{
    /// <summary>
    /// Manages backend operations for a job queue
    /// </summary>
    public interface IScheduledJobRepository
    {
        void Save(ScheduledJob job, RepeatInfo info = null);
        void Delete(ScheduledJob job);
        IList<ScheduledJob> GetNextAvailable(int readAhead);
        IList<ScheduledJob> GetAll();
        Batch CreateBatch(string name, IEnumerable<ScheduledJob> jobs);
    }
}