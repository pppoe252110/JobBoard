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
    public override void Configure()
    {
        Get("/resumes/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResumeSearchRequest req, CancellationToken ct)
    {
        var index = meilisearchClient.Index("resumes");

        var query = string.IsNullOrWhiteSpace(req.Query) ? "" : req.Query.Trim();

        var filterParts = new List<string>();

        // Only show visible resumes
        filterParts.Add("isVisible = true");

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
            Offset = (req.PageNumber - 1) * req.PageSize,
            Limit = req.PageSize,
            Sort = new[] { "updatedAt:desc" },
            Filter = filter
        };

        // Search using the camelCase DTO
        var result = await index.SearchAsync<ResumeSearchHit>(query, searchOptions, ct);

        // Map to the response DTO
        var items = result.Hits.Select(hit => new ResumeSummaryResponse(
            hit.id,
            hit.title,
            hit.experienceYears,
            hit.expectedSalary
        )).ToList();

        // Use EstimatedTotalHits for correct total count across pages
        var totalCount = result.Hits.Count;

        await Send.OkAsync(new PagedResponse<ResumeSummaryResponse>(
            items, totalCount, req.PageNumber, req.PageSize), cancellation: ct);
    }
}