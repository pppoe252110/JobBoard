using JobBoard.ApiService.Features.Resumes.Models;

public class Resume
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? ExpectedSalary { get; set; }
    public List<ContactMethodDto> ContactMethods { get; set; } = new();
    public SkillsMatrix Skills { get; set; } = new();
    public bool IsVisible { get; set; }
    public string AboutMe { get; set; } = string.Empty;
    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int ExperienceYears { get; set; }
}