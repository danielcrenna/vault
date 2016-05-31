using System.Collections.Generic;

namespace cohort.Models
{
    public interface ILogRepository
    {
        void Save(Log error);
        IEnumerable<Log> GetAll();
    }
}