namespace JobBoard.Web.Models;

public class VacancySearchResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = "";
    public string DescriptionMarkdown { get; set; } = "";
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public string Location { get; set; } = "";
    public bool IsRemote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}