using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Paging
{
    /// <summary>
    /// A wrapper around an IEnumerable that may contain <see cref="IPageable" /> data.
    /// This is useful when you need to return an empty collection to a method expecting
    /// an <see cref="IPageable" /> slice, or if your data access layer does not
    /// make use of <see cref="IQueryable" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedEnumerable<T> : IPagedEnumerable<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int PageStart { get; set; }
        public int PageEnd { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        private readonly IEnumerable<T> _source;

        public PagedEnumerable()
        {
            _source = new List<T>(0);
        }

        public PagedEnumerable(IEnumerable<T> source)
        {
            _source = source;
        }

        public PagedEnumerable(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount) : this(source)
        {
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);
            HasPreviousPage = pageIndex > 1;
            HasNextPage = pageIndex < TotalPages;
            PageIndex = pageIndex;
            PageSize = pageSize;
            PageStart = PageSize * PageIndex - PageSize + 1;
            PageCount = _source.Count();
            PageEnd = PageCount == PageSize
                                 ? PageStart + PageSize - 1
                                 : PageStart + PageCount - 1;
        }

        public PagedEnumerable(IPagedEnumerable<T> source)
        {
            _source = source;
            PageIndex = source.PageIndex;
            PageSize = source.PageSize;
            PageCount = source.PageCount;
            PageStart = source.PageStart;
            PageEnd = source.PageEnd;
            TotalCount = source.TotalCount;
            TotalPages = source.TotalPages;
            HasPreviousPage = source.HasPreviousPage;
            HasNextPage = source.HasNextPage;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}