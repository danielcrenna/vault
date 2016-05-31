namespace Paging
{
    /// <summary>
    /// Representing a data slice of a whole for paging functions
    /// </summary>
    public interface IPageable
    {
        /// <summary>
        /// The page this data slice represents
        /// </summary>
        int PageIndex { get; }

        /// <summary>
        /// The requested slice size
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// The actual slice size (sometimes the last page has fewer results)
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// The index to the whole that the first item in this slice represents,
        /// i.e. If the slice contains results 11 to 60, this value is 11.
        /// </summary>
        int PageStart { get; }

        /// <summary>
        /// The index to the whole that the last item in this slice represents
        /// i.e. If the slice contains results 11 to 60, this value is 60.
        /// </summary>
        int PageEnd { get; }

        /// <summary>
        /// The total number of results in the entire data source
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// The total number of pages in the entire data source, based on <see cref="PageSize" />
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// True, if the slice has previous pages available
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// True, if the slice has subsequent pages available
        /// </summary>
        bool HasNextPage { get; }
    }
}