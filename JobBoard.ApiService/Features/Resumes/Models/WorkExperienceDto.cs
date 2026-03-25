namespace JobBoard.ApiService.Features.Resumes.Models;

public class WorkExperienceDto
{
    public Guid? Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
}