namespace PositiveNews.Application.Constants;

/// <summary>
/// Default and bounded pagination values for article feed queries.
/// </summary>
public static class PaginationConstants
{
    /// <summary>Compile-time default page size for tests and direct MediatR calls.</summary>
    public const int DefaultPageSize = 10;

    /// <summary>Maximum allowed page size for feed queries.</summary>
    public const int MaxPageSize = 100;
}
