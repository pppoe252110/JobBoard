using FastEndpoints;
using JobBoard.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Vacancies;

public class DeleteVacancyEndpoint : EndpointWithoutRequest
{
    private readonly JobPortalDbContext _db;

    public DeleteVacancyEndpoint(JobPortalDbContext db) => _db = db;

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
        await Send.NoContentAsync(ct);
    }
}