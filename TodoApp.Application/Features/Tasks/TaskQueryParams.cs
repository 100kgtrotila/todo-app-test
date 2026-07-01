namespace TodoApp.Application.Features.Tasks;

public class TaskQueryParams
{
    public int Page { get; set; } = 1;
    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 100);
    }
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsCompleted { get; set; }
    public string? SortBy { get; set; } = "createdat";
    public bool SortDescending { get; set; } = true;
}
