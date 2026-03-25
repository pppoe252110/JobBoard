namespace JobBoard.ApiService.Features.Resumes.Models;

public class SkillsMatrix
{
    public List<LanguageDto> Languages { get; set; } = new();
    public List<string> Frameworks { get; set; } = new();
}