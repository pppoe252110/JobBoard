namespace JobBoard.Web.Models;

public class VacancySearchModel
{
    public string? SearchTerm { get; set; }
    public decimal? MinSalary { get; set; }
    public string? Location { get; set; }
    public bool? IsRemote { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}