namespace ShopAPI.Common.Pagination
{
    /// <summary>
    /// Base class for pagination parameters
    /// </summary>
    public class PaginationParams
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Current page number (1-based, default: 1)
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Items per page (default: 10, max: 100)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 100 ? 100 : (value < 1 ? 10 : value);
        }

        /// <summary>
        /// Field to sort by
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort order: 'asc' or 'desc' (default: asc)
        /// </summary>
        public string SortOrder { get; set; } = "asc";

        /// <summary>
        /// Search term for filtering
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total count of all items
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total pages available
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Page data
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// Parameters for product filtering and pagination
    /// </summary>
    public class ProductFilterParams : PaginationParams
    {
        /// <summary>
        /// Minimum price filter
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Maximum price filter
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Category ID filter
        /// </summary>
        public int? CategoryId { get; set; }
    }
}
