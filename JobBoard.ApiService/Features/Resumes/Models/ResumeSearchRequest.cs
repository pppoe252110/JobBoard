namespace JobBoard.ApiService.Features.Resumes.Models;

public record ResumeSearchRequest(
    string? Query,
    int? MinExperience,
    string? RequiredSkill,
    int PageNumber = 1,
    int PageSize = 10);

public record ResumeSummaryResponse(Guid Id, string Title, int ExperienceYears, decimal? ExpectedSalary);