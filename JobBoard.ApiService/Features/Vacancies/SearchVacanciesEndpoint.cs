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
        var docs = await index.GetDocumentsAsync<object>(cancellationToken: ct);
        Console.WriteLine(JsonSerializer.Serialize(docs));

        var query = string.IsNullOrWhiteSpace(req.SearchTerm) ? "" : req.SearchTerm.Trim();

        var filterParts = new List<string>();

        // MATCH THE SETTINGS: PascalCase
        filterParts.Add("isArchived = false");

        if (req.IsRemote.HasValue)
        {
            filterParts.Add($"isRemote = {req.IsRemote.Value.ToString().ToLowerInvariant()}");
        }

        if (req.MinSalary.HasValue)
        {
            filterParts.Add($"salaryFrom >= {req.MinSalary.Value}");
        }

        if (!string.IsNullOrWhiteSpace(req.Location))
        {
            var escapedLocation = req.Location.Replace("'", "\\'");
            filterParts.Add($"location = '{escapedLocation}'");
        }

        var filter = filterParts.Count > 0 ? string.Join(" AND ", filterParts) : null;

        var searchOptions = new SearchQuery
        {
            Offset = (req.PageNumber - 1) * req.PageSize,
            Limit = req.PageSize,
            // MATCH THE SETTINGS: PascalCase (Make sure 'CreatedAt' is in your Sortable settings!)
            Sort = new[] { "createdAt:desc" },
            Filter = filter
        };

        var result = await index.SearchAsync<Vacancy>(query, searchOptions, ct);

        var items = result.Hits.Select(hit => new VacancySearchResponse(
            hit.Id,
            hit.Title,
            hit.SalaryFrom,
            hit.SalaryTo,
            hit.Location,
            hit.IsRemote,
            hit.CreatedAt
        )).ToList();

        // EXTRACT TOTAL COUNT PROPERLY
        int totalCount = 0;
        if (result is SearchResult<Vacancy> standardResult)
        {
            totalCount = standardResult.EstimatedTotalHits;
        }
        // Temporarily ignore all filters and limits to see if the index is empty
        var allDocs = await index.SearchAsync<Vacancy>("", new SearchQuery { Limit = 10 }, ct);
        foreach (var hit in allDocs.Hits)
        {
            Console.WriteLine($"Id: {hit.Id}, Title: {hit.Title}, IsArchived: {hit.IsArchived}");
        }

        Console.WriteLine("Found Documents: " + totalCount);

        await Send.OkAsync(new PagedResponse<VacancySearchResponse>(
            items, totalCount, req.PageNumber, req.PageSize), cancellation: ct);
    }
}