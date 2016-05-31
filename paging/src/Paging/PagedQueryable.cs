using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paging
{
    /// <summary>
    /// A wrapper around an IQueryable source that is evaluated only once, and provides
    /// efficient calculations for <see cref="IPageable" /> consumers. This wrapper
    /// assumes a 1-based page index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedQueryable<T> : IPagedQueryable<T>
    {
        private readonly IQueryable<T> _source;

        // Caches result if page slice is ever evaluated
        private IEnumerable<T> _sliced;

        // Caches result if source count is ever evaluated
        private int? _sourceCount;

        // Caches result if slice count is ever evaluated
        private int? _sliceCount;

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int PageStart { get; private set; }

        private int? _pageCount;
        public int PageCount
        {
            get
            {
                if (!_pageCount.HasValue)
                {
                    CalculateTotals(_source);

                    _pageCount = GetSliceCount();
                }

                return _pageCount.Value;
            }
        }

        private int? _pageEnd;
        public int PageEnd
        {
            get
            {
                if (!_pageEnd.HasValue)
                {
                    CalculateTotals(_source);

                    _pageCount = GetSliceCount();

                    _pageEnd = _pageCount == PageSize
                                   ? PageStart + PageSize
                                   : PageStart + _pageCount;
                }

                return _pageEnd.HasValue ? _pageEnd.Value - 1 : -1;
            }
        }

        private int? _totalCount;
        public int TotalCount
        {
            get
            {
                if (!_totalCount.HasValue)
                {
                    CalculateTotals(_source);
                }

                return _totalCount != null
                           ? _totalCount.Value
                           : 0;
            }
        }

        private int? _totalPages;
        public int TotalPages
        {
            get
            {
                if (!_totalPages.HasValue)
                {
                    CalculateTotals(_source);
                }
                return _totalPages != null
                           ? _totalPages.Value
                           : 0;
            }
        }

        public PagedQueryable(IQueryable<T> source, int? page, int? count)
        {
            _source = source;

            if (page.HasValue)
            {
                // Wrapper is 1-based
                PageIndex = page.Value <= 0 ? 1 : page.Value;
            }
            else
            {
                PageIndex = 1;
            }

            PageSize = !count.HasValue ? Paging.DefaultPageSize : count.Value;

            PageStart = PageSize * PageIndex - PageSize + 1;
        }

        public IPagedQueryable<T> NextPage
        {
            get
            {
                return new PagedQueryable<T>(_source, PageIndex + 1, PageSize);
            }
        }

        public IPagedQueryable<T> PreviousPage
        {
            get
            {
                return new PagedQueryable<T>(_source, PageIndex - 1, PageSize);
            }
        }

        private void CalculateTotals(IQueryable<T> source)
        {
            if (_sourceCount == null)
            {
                _sourceCount = source.Count();
            }
            _totalCount = _sourceCount;
            _totalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        }

        public bool HasPreviousPage
        {
            get { return (PageIndex > 1); }
        }

        public bool HasNextPage
        {
            get { return (PageIndex < TotalPages); }
        }

        public Expression Expression
        {
            get { return _source.Expression; }
        }

        public Type ElementType
        {
            get { return _source.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return _source.Provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetSlice().GetEnumerator();
        }

        private int GetSliceCount()
        {
            return _sliceCount ?? (_sliceCount = SliceQuery().Count()).Value;
        }

        private IEnumerable<T> GetSlice()
        {
            // Always causes an evaluation of the source query
            return _sliced ?? (_sliced = SliceQuery().ToList());
        }

        private IQueryable<T> SliceQuery()
        {
            return _source.Skip((PageIndex - 1) * PageSize).Take(PageSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}