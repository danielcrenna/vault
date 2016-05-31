using System.Collections.Generic;
using Dapper;
using cohort.Models;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class LogRepository : ILogRepository
    {
        public void Save(Log log)
        {
            log.Id = 0;
            UnitOfWork.Current.Insert(log);
        }

        public IEnumerable<Log> GetAll()
        {
            return UnitOfWork.Current.Query<Log>("SELECT * FROM Log");
        }
    }
}