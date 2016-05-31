using cohort.Models;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class ErrorRepository : IErrorRepository
    {
        public void Save(Error error)
        {
            error.Id = 0;
            UnitOfWork.Current.Insert(error);
        }
    }
}