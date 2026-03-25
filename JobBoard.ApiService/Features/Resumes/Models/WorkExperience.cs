namespace JobBoard.ApiService.Features.Resumes.Models;

public class WorkExperience
{
    public Guid Id { get; set; }
    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new(); // JSONB
}