using System.Collections.Generic;
using Paging;

namespace cohort.Api.Models
{
    public class ResourceCollection<T>
    {
        public PagedEnumerable<T> Data { get; set; }
        public int TotalCount
        {
            get { return Data.TotalCount; }
        }
        public int TotalPages
        {
            get { return Data.TotalPages; }
        }
        public bool HasNextPage
        {
            get { return Data.HasNextPage; }
        }
        public bool HasPreviousPage
        {
            get { return Data.HasPreviousPage; }
        }
        public int PageIndex
        {
            get { return Data.PageIndex; }
        }
        public int PageCount
        {
            get { return Data.PageCount; }
        }
        public int PageSize
        {
            get { return Data.PageSize; }
        }
        public int PageStart
        {
            get { return Data.PageStart; }
        }
        public int PageEnd
        {
            get { return Data.PageEnd; }
        }
        public string NextPage { get; set; }
        public string PreviousPage { get; set; }
        public ResourceCollection(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Data = new PagedEnumerable<T>(source, pageIndex, pageSize, totalCount);
        }
    }
}