namespace Application.Common.Models;

public class PagingParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string OrderBy { get; set; } = "Id"; // Default column to sort by
    public bool IsDescending { get; set; } = false;
}
