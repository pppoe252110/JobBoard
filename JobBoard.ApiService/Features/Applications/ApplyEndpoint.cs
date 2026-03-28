using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Applications;

public class ApplyRequest
{
    public Guid ResumeId { get; set; }
}

public class ApplyEndpoint : Endpoint<ApplyRequest>
{
    private readonly JobPortalDbContext _db;

    public ApplyEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/vacancies/{vacancyId}/apply");
        Roles("User");
    }

    public override async Task HandleAsync(ApplyRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var vacancyId = Route<Guid>("vacancyId");

        // Check if vacancy exists and not archived
        var vacancy = await _db.Vacancies.FindAsync(new object[] { vacancyId }, ct);
        if (vacancy == null || vacancy.IsArchived)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Prevent user from applying to their own vacancy
        if (vacancy.UserId == userId)
        {
            AddError("You cannot apply to your own vacancy.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        // Check if resume belongs to user
        var resume = await _db.Resumes.FindAsync(new object[] { req.ResumeId }, ct);
        if (resume == null || resume.UserId != userId)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Check if already applied
        var alreadyApplied = await _db.Applications
            .AnyAsync(a => a.VacancyId == vacancyId && a.ResumeId == req.ResumeId, ct);

        if (alreadyApplied)
        {
            AddError("You have already applied to this vacancy.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var application = new Application
        {
            Id = Guid.NewGuid(),
            VacancyId = vacancyId,
            ResumeId = req.ResumeId,
            AppliedAt = DateTimeOffset.UtcNow
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync(ct);

        await Send.OkAsync(new { Message = "Application submitted" }, ct);
    }
}