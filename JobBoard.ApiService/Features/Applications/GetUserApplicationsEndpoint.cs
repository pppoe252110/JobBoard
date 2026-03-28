using FastEndpoints;
using JobBoard.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Applications;

public class GetUserApplicationsEndpoint : EndpointWithoutRequest<List<MyApplicationDto>>
{
    private readonly JobPortalDbContext _db;
    public GetUserApplicationsEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/profile/applications");
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

        var applications = await _db.Applications
            .Join(_db.Resumes, app => app.ResumeId, res => res.Id, (app, res) => new { app, res })
            .Where(x => x.res.UserId == userId)
            .Join(_db.Vacancies, x => x.app.VacancyId, vac => vac.Id, (x, vac) => new { x.app, vac })
            .OrderByDescending(x => x.app.AppliedAt)
            .Select(x => new MyApplicationDto(
                x.app.Id,
                x.vac.Id,
                x.vac.Title,
                x.app.AppliedAt,
                x.app.Status
            ))
            .ToListAsync(ct);

        await Send.OkAsync(applications, ct);
    }
}

public record MyApplicationDto(Guid ApplicationId, Guid VacancyId, string VacancyTitle, DateTimeOffset AppliedAt, string Status);