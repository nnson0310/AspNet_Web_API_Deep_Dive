namespace CourseLibrary.API.Profiles;

public class AuthorResourcesParameters
{
    private readonly int _maxPageSize = 20;

    public string? MainCategory { get; set; }

    public string? SearchQuery { get; set; }

    public int PageNumber { get; set; } = 1;

    private int _pageSize { get; set; } = 10;

    public string OrderBy { get; set; } = "Name";

    public string? Fields { get; set;}

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > _maxPageSize) ? _maxPageSize : value;
    }
}

