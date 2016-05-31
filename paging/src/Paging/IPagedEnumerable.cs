using System.Collections.Generic;

namespace Paging
{
    public interface IPagedEnumerable<out T> : IPageable, IEnumerable<T>
    {

    }
}