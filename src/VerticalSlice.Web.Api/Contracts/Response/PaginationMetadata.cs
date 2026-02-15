namespace VerticalSlice.Web.Api.Contracts.Response;

public class PaginationMetadata
{
    /// <summary>
    ///     Current page number (1-indexed)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    ///     Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    ///     Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    ///     Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    ///     Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }
}
