namespace JobBoard.ApiService.Features.Resumes.Models;

public class CreateResumeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<ContactMethodDto> ContactMethods { get; set; } = new();
    public SkillsMatrix Skills { get; set; } = new();
    public bool IsVisible { get; set; }
    public string AboutMe { get; set; } = string.Empty;
    public List<WorkExperienceDto> WorkExperiences { get; set; } = new();
}