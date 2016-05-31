using System.Linq;

namespace Paging
{
    public interface IPagedQueryable<out T> : IQueryable<T>, IPagedEnumerable<T>
    {
        IPagedQueryable<T> NextPage { get; }
        IPagedQueryable<T> PreviousPage { get; }
    }
}