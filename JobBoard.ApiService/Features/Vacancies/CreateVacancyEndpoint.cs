using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Vacancies.Models;
using JobBoard.ApiService.Utils;
using Meilisearch;

namespace JobBoard.ApiService.Features.Vacancies;

public class CreateVacancyEndpoint : Endpoint<CreateVacancyRequest>
{
    private readonly JobPortalDbContext _db;
    private readonly MeilisearchClient _meilisearchClient;

    public CreateVacancyEndpoint(JobPortalDbContext db, MeilisearchClient meilisearchClient)
    {
        _db = db;
        _meilisearchClient = meilisearchClient;
    }

    public override void Configure()
    {
        Post("/vacancies");
        Roles("User");
    }

    public override async Task HandleAsync(CreateVacancyRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = req.Title,
            DescriptionMarkdown = req.DescriptionMarkdown,
            SalaryFrom = req.SalaryFrom,
            SalaryTo = req.SalaryTo,
            Location = req.Location ?? string.Empty,
            IsRemote = req.IsRemote,
            IsArchived = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Vacancies.Add(vacancy);
        await _db.SaveChangesAsync(ct);

        // Index the vacancy in Meilisearch
        var index = _meilisearchClient.Index("vacancies");
        var taskInfo = await index.AddDocumentsAsync([MeilisearchConverter.ConvertVacancy(vacancy)], cancellationToken: ct);

        await Send.OkAsync(new { vacancy.Id }, ct);
    }
}