using FastEndpoints;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Features.Vacancies.Models;
using Meilisearch;
using System.Text.Json;

namespace JobBoard.ApiService.Features.Vacancies;

public record VacancySearchRequest(
    string? SearchTerm,
    decimal? MinSalary,
    string? Location,
    bool? IsRemote,
    int PageNumber = 1,
    int PageSize = 10);

public record VacancySearchResponse(
    Guid Id,
    Guid UserId,
    string Title,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string Location,
    bool IsRemote,
    DateTimeOffset CreatedAt);

public class SearchVacanciesEndpoint(MeilisearchClient meilisearchClient)
    : Endpoint<VacancySearchRequest, PagedResponse<VacancySearchResponse>>
{
    public override void Configure()
    {
        Get("/vacancies/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(VacancySearchRequest req, CancellationToken ct)
    {
        var index = meilisearchClient.Index("vacancies");

        // 1. Combine SearchTerm and Location for Fuzzy Search
        // This allows users to find "New York" by typing "New"
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) queryParts.Add(req.SearchTerm.Trim());
        if (!string.IsNullOrWhiteSpace(req.Location)) queryParts.Add(req.Location.Trim());

        var fullQuery = string.Join(" ", queryParts);

        // 2. Build strict filters (Booleans and Numbers only)
        var filterParts = new List<string>();
        filterParts.Add("isArchived = false");

        if (req.IsRemote.HasValue)
        {
            filterParts.Add($"isRemote = {req.IsRemote.Value.ToString().ToLowerInvariant()}");
        }

        if (req.MinSalary.HasValue)
        {
            // Note: Meilisearch uses the property names from your Converter (lowercase 's')
            filterParts.Add($"salaryFrom >= {req.MinSalary.Value}");
        }

        var searchOptions = new SearchQuery
        {
            Offset = (Math.Max(1, req.PageNumber) - 1) * req.PageSize,
            Limit = req.PageSize,
            Sort = new[] { "createdAt:desc" },
            Filter = filterParts.Count > 0 ? string.Join(" AND ", filterParts) : null
        };

        // 3. Use VacancySearchResponse as the T for the search to ensure mapping works
        var result = await index.SearchAsync<VacancySearchResponse>(fullQuery, searchOptions, ct);
        
        // 4. Properly extract count
        // Meilisearch returns 'EstimatedTotalHits' by default for performance
        int totalCount = result.Hits.Count;

        Console.WriteLine((await index.GetDictionaryAsync()).Count()+"AAAAAAAAAAAAAAAAAAA"+totalCount);

        await Send.OkAsync(new PagedResponse<VacancySearchResponse>(
            result.Hits.ToList(),
            totalCount,
            req.PageNumber,
            req.PageSize),
            cancellation: ct);
    }
}