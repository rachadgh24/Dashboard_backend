namespace task1.Application;

/// <summary>Normalizes paging inputs before they reach repositories (no fixed page sizes in data access).</summary>
public static class PaginationQuery
{
    public static int NormalizePage(int page) => page < 1 ? 1 : page;

    public static int NormalizePageSize(int pageSize)
    {
        if (pageSize < 1) return 1;
        if (pageSize > 100) return 100;
        return pageSize;
    }
}
