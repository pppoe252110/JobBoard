using JobBoard.Web.Models;

namespace JobBoard.Web.Services;

public class ResumeApiClient(HttpClient httpClient)
{
    public async Task<List<ResumeResponse>> GetAllAsync() =>
        await httpClient.GetFromJsonAsync<List<ResumeResponse>>("/resumes") ?? new();

    public async Task<List<ResumeResponse>> GetUserResumesAsync() =>
        await httpClient.GetFromJsonAsync<List<ResumeResponse>>("/profile/resumes") ?? new();

    public async Task<HttpResponseMessage> CreateAsync(CreateResumeRequest request) =>
        await httpClient.PostAsJsonAsync("/resumes", request);

    public async Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateResumeRequest request) =>
        await httpClient.PutAsJsonAsync($"/resumes/{id}", request);

    public async Task<HttpResponseMessage> DeleteAsync(Guid id) =>
        await httpClient.DeleteAsync($"/resumes/{id}");

    public async Task<ResumeResponse?> GetByIdAsync(Guid id)
    {
        return await httpClient.GetFromJsonAsync<ResumeResponse>($"/resumes/{id}");
    }

    public async Task<PagedResponse<ResumeSummaryResponse>> SearchResumesAsync(ResumeSearchModel search)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search.Query))
            query.Add($"query={Uri.EscapeDataString(search.Query)}");
        if (search.MinExperience.HasValue)
            query.Add($"minExperience={search.MinExperience}");
        if (!string.IsNullOrWhiteSpace(search.RequiredSkill))
            query.Add($"requiredSkill={Uri.EscapeDataString(search.RequiredSkill)}");

        query.Add($"pageNumber={search.PageNumber}");
        query.Add($"pageSize={search.PageSize}");

        var url = $"/resumes/search?{string.Join("&", query)}";

        return await httpClient.GetFromJsonAsync<PagedResponse<ResumeSummaryResponse>>(url)
               ?? new PagedResponse<ResumeSummaryResponse>
               {
                   Items = new List<ResumeSummaryResponse>(),
                   TotalCount = 0,
                   PageNumber = search.PageNumber,
                   PageSize = search.PageSize
               };
    }
}

// DTOs
public class LanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public class ContactMethodDto
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
public record SkillsMatrixDto(List<LanguageDto> Languages, List<string> HardSkills);

public record UpdateResumeRequest(
    Guid Id,
    string FullName,
    string Title,
    string Location,
    decimal? ExpectedSalary,
    List<ContactMethodDto> ContactMethods,
    SkillsMatrixDto Skills,
    bool IsVisible,
    string AboutMe,
    List<WorkExperienceDto> WorkExperiences
);

public record CreateResumeRequest(
    string FullName,
    string Title,
    string Location,
    decimal? ExpectedSalary,
    List<ContactMethodDto> ContactMethods,
    SkillsMatrixDto Skills,
    bool IsVisible,
    string AboutMe,
    List<WorkExperienceDto> WorkExperiences
);

public record ResumeResponse(
    Guid Id,
    string FullName,
    string Title,
    string Location,
    decimal? ExpectedSalary,
    List<ContactMethodDto> ContactMethods,
    SkillsMatrixDto Skills,
    bool IsVisible,
    string AboutMe,
    List<WorkExperienceDto> WorkExperiences
);

public record WorkExperienceDto(
    Guid? Id,
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Description,
    List<string> Technologies
);