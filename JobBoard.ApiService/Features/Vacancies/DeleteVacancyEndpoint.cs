using FastEndpoints;
using JobBoard.ApiService.Data;
using Meilisearch;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Vacancies;

public class DeleteVacancyEndpoint : EndpointWithoutRequest
{
    private readonly JobPortalDbContext _db;
    private readonly MeilisearchClient _meilisearchClient;

    public DeleteVacancyEndpoint(JobPortalDbContext db, MeilisearchClient meilisearchClient)
    {
        _db = db;
        _meilisearchClient = meilisearchClient;
    }

    public override void Configure()
    {
        Delete("/vacancies/{id}");
        Roles("User");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var id = Route<Guid>("id");
        var vacancy = await _db.Vacancies.FindAsync(new object[] { id }, ct);
        if (vacancy == null || vacancy.UserId != userId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        _db.Vacancies.Remove(vacancy);
        await _db.SaveChangesAsync(ct);

        var index = _meilisearchClient.Index("vacancies");
        await index.DeleteOneDocumentAsync(id.ToString(), cancellationToken: ct);

        await Send.NoContentAsync(ct);
    }
}