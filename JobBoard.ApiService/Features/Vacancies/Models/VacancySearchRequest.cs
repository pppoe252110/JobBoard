namespace JobBoard.ApiService.Features.Vacancies.Models;

public record VacancySearchRequest(
    string? SearchTerm,
    decimal? MinSalary,
    string? Location,
    bool? IsRemote,
    int PageNumber = 1,
    int PageSize = 10);

public record VacancySearchResponse(
    Guid Id,
    string Title,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string Location,
    bool IsRemote,
    DateTimeOffset CreatedAt);