using FastEndpoints;
using Meilisearch;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Features.Resumes.Models;

namespace JobBoard.ApiService.Features.Resumes;

// DTO for search results – matches the camelCase document structure
public record ResumeSearchHit(
    Guid id,
    string title,
    int experienceYears,
    decimal? expectedSalary
);

public class SearchResumesEndpoint(MeilisearchClient meilisearchClient)
    : Endpoint<ResumeSearchRequest, PagedResponse<ResumeSummaryResponse>>
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    public override void Configure()
    {
        Get("/resumes/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResumeSearchRequest req, CancellationToken ct)
    {
        // Clamp page size
        int pageSize = req.PageSize;
        if (pageSize < 1) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        // Clamp page number (minimum 1)
        int pageNumber = req.PageNumber < 1 ? 1 : req.PageNumber;

        var index = meilisearchClient.Index("resumes");
        var query = string.IsNullOrWhiteSpace(req.Query) ? "" : req.Query.Trim();

        var filterParts = new List<string> { "isVisible = true" };
        if (req.MinExperience.HasValue)
        {
            filterParts.Add($"experienceYears >= {req.MinExperience.Value}");
        }
        if (!string.IsNullOrWhiteSpace(req.RequiredSkill))
        {
            var escapedSkill = req.RequiredSkill.Replace("'", "\\'");
            filterParts.Add($"skills.hardSkills = '{escapedSkill}'");
        }
        var filter = filterParts.Count > 0 ? string.Join(" AND ", filterParts) : null;

        var searchOptions = new SearchQuery
        {
            Offset = (pageNumber - 1) * pageSize,
            Limit = pageSize,
            Sort = ["updatedAt:desc"],
            Filter = filter
        };

        // Execute search
        var result = await index.SearchAsync<ResumeSearchHit>(query, searchOptions, ct);

        // Get correct total count
        long totalCount = result.Hits.Count;

        // Clamp page number after we know total count
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
            // Re-run search with corrected page number (optional, but safe)
            searchOptions.Offset = (pageNumber - 1) * pageSize;
            result = await index.SearchAsync<ResumeSearchHit>(query, searchOptions, ct);
        }

        // Map hits to response DTO
        var items = result.Hits.Select(hit => new ResumeSummaryResponse(
            hit.id,
            hit.title,
            hit.experienceYears,
            hit.expectedSalary
        )).ToList();

        // Return paged response
        await Send.OkAsync(new PagedResponse<ResumeSummaryResponse>(
            items, (int)totalCount, pageNumber, pageSize), ct);
    }
}