namespace VerticalSlice.Web.Api.Contracts.Response;

public class PagedResponse<T>
{
    /// <summary>
    ///     The data items for the current page
    /// </summary>
    public IEnumerable<T> Data { get; set; } = Array.Empty<T>();

    /// <summary>
    ///     Pagination metadata
    /// </summary>
    public PaginationMetadata Pagination { get; set; } = new();
}
