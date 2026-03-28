using FastEndpoints;
using JobBoard.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Applications;

public class GetVacancyApplicationsEndpoint : EndpointWithoutRequest<List<ApplicationDto>>
{
    private readonly JobPortalDbContext _db;

    public GetVacancyApplicationsEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/vacancies/{vacancyId}/applications");
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

        var vacancyId = Route<Guid>("vacancyId");

        // 1. Verify ownership of the vacancy first
        var ownsVacancy = await _db.Vacancies
            .AnyAsync(v => v.Id == vacancyId && v.UserId == userId, ct);

        if (!ownsVacancy)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 2. Perform the Join and OrderBy BEFORE selecting into the DTO
        // This ensures EF Core can translate the sorting and joining to SQL easily
        var applications = await _db.Applications
            .Where(a => a.VacancyId == vacancyId)
            .Join(_db.Resumes,
                app => app.ResumeId,
                res => res.Id,
                (app, res) => new { app, res }) // Create a temporary anonymous object
            .OrderByDescending(x => x.app.AppliedAt) // Sort by the actual DB property
            .Select(x => new ApplicationDto(
                x.app.Id,
                x.app.VacancyId,
                x.app.ResumeId,
                x.res.UserId,
                x.res.FullName,
                x.res.Title,
                x.app.AppliedAt,
                x.app.Status
            ))
            .ToListAsync(ct);

        await Send.OkAsync(applications, ct);
    }
}

public record ApplicationDto(Guid Id, Guid VacancyId, Guid ResumeId, Guid ApplicantUserId, string ApplicantName, string ResumeTitle, DateTimeOffset AppliedAt, string Status);