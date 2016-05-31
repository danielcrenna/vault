using System;
using System.Linq.Expressions;

namespace cohort.Api.Models
{
    public interface IResourceRepository<T>
    {
        ResourceCollection<T> Get(int? page, int? count, Expression<Func<T, object>> sortedOn = null);
        T Get(int id);
        void Save(T resource);
    }
}