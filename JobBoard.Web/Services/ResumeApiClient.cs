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
        return await httpClient.GetFromJsonAsync<ResumeResponse>($"/api/resumes/{id}");
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
public record SkillsMatrixDto(List<LanguageDto> Languages, List<string> Frameworks);

public record CreateResumeRequest(
    string FullName,
    string Title,
    List<ContactMethodDto> ContactMethods,
    SkillsMatrixDto Skills,
    bool IsVisible,
    string AboutMe,
    List<WorkExperienceDto> WorkExperiences
);

public record UpdateResumeRequest(
    Guid Id,
    string FullName,
    string Title,
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