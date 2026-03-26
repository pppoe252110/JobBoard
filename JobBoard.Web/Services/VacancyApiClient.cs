using JobBoard.Web.Models;

namespace JobBoard.Web.Services;

public class VacancyApiClient(HttpClient httpClient)
{
    public async Task<List<ApplicationDto>> GetApplicationsAsync(Guid vacancyId) =>
        await httpClient.GetFromJsonAsync<List<ApplicationDto>>($"/vacancies/{vacancyId}/applications") ?? new();

    public async Task<HttpResponseMessage> UpdateApplicationStatusAsync(Guid applicationId, string status) =>
        await httpClient.PatchAsJsonAsync($"/applications/{applicationId}/status", new { Status = status });

    public async Task<VacancyResponse?> GetByIdAsync(Guid id) =>
    await httpClient.GetFromJsonAsync<VacancyResponse>($"/vacancies/{id}");

    public async Task<List<VacancyResponse>> GetAllAsync() =>
        await httpClient.GetFromJsonAsync<List<VacancyResponse>>("/vacancies") ?? new();

    public async Task<List<VacancyResponse>> GetUserVacanciesAsync() =>
        await httpClient.GetFromJsonAsync<List<VacancyResponse>>("/profile/vacancies") ?? new();

    public async Task<HttpResponseMessage> CreateAsync(CreateVacancyRequest request) =>
        await httpClient.PostAsJsonAsync("/vacancies", request);

    public async Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateVacancyRequest request) =>
        await httpClient.PutAsJsonAsync($"/vacancies/{id}", request);

    public async Task<HttpResponseMessage> DeleteAsync(Guid id) =>
        await httpClient.DeleteAsync($"/vacancies/{id}");

    public async Task<HttpResponseMessage> ApplyAsync(Guid vacancyId, Guid resumeId) =>
        await httpClient.PostAsJsonAsync($"/vacancies/{vacancyId}/apply", new { ResumeId = resumeId });

    public async Task<PagedResponse<VacancySearchResponse>> SearchVacanciesAsync(VacancySearchModel search)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            query.Add($"searchTerm={Uri.EscapeDataString(search.SearchTerm)}");
        if (search.MinSalary.HasValue)
            query.Add($"minSalary={search.MinSalary}");
        if (!string.IsNullOrWhiteSpace(search.Location))
            query.Add($"location={Uri.EscapeDataString(search.Location)}");
        if (search.IsRemote.HasValue)
            query.Add($"isRemote={search.IsRemote}");
        query.Add($"pageNumber={search.PageNumber}");
        query.Add($"pageSize={search.PageSize}");

        var url = $"/vacancies/search?{string.Join("&", query)}";
        return await httpClient.GetFromJsonAsync<PagedResponse<VacancySearchResponse>>(url)
               ?? new PagedResponse<VacancySearchResponse> { Items = new List<VacancySearchResponse>(), TotalCount = 0, PageNumber = search.PageNumber, PageSize = search.PageSize };
    }
}

// DTOs
public record ApplicationDto(
    Guid Id,
    Guid VacancyId,
    Guid ResumeId,
    string ApplicantName,
    string ResumeTitle,
    DateTimeOffset AppliedAt,
    string Status
);

public record CreateVacancyRequest(
    string Title,
    string DescriptionMarkdown,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string? Location,
    bool IsRemote
);

public record UpdateVacancyRequest(
    Guid Id,
    string Title,
    string DescriptionMarkdown,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string? Location,
    bool IsRemote
);

public record VacancyResponse(
    Guid Id,
    string Title,
    string DescriptionMarkdown,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string? Location,
    bool IsRemote,
    DateTimeOffset CreatedAt,
    bool IsArchived
);