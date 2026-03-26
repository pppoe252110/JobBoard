namespace JobBoard.Web.Models;

public class ResumeSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal? ExpectedSalary { get; set; }
}