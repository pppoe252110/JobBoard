namespace JobBoard.Web.Services;

public class VacancyApiClient(HttpClient httpClient)
{
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
}

// DTOs
public record CreateVacancyRequest(string Title, string DescriptionMarkdown, decimal? SalaryFrom, decimal? SalaryTo);
public record UpdateVacancyRequest(Guid Id, string Title, string DescriptionMarkdown, decimal? SalaryFrom, decimal? SalaryTo);
public record VacancyResponse(Guid Id, string Title, string DescriptionMarkdown, decimal? SalaryFrom, decimal? SalaryTo, DateTimeOffset CreatedAt, bool IsArchived);