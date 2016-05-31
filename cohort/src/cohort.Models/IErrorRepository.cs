using cohort.Models;

namespace cohort
{
    public interface IErrorRepository
    {
        void Save(Error error);
    }
}