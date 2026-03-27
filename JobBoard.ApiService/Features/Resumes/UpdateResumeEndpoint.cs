using FastEndpoints;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Resumes.Models;
using JobBoard.ApiService.Utils;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobBoard.ApiService.Features.Resumes;

public class UpdateResumeEndpoint : Endpoint<UpdateResumeRequest>
{
    private readonly JobPortalDbContext _db;
    private readonly MeilisearchClient _meilisearchClient;

    public UpdateResumeEndpoint(JobPortalDbContext db, MeilisearchClient meilisearchClient)
    {
        _db = db;
        _meilisearchClient = meilisearchClient;
    }

    public override void Configure()
    {
        Put("/resumes/{id}");
        Roles("User");
    }

    public override async Task HandleAsync(UpdateResumeRequest req, CancellationToken ct)
    {
        var resumeId = Route<Guid>("id");
        if (req.Id != resumeId)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        // 1. Authenticate & Fetch (Combining ownership check in query)
        var userIdClaim = User.FindFirstValue("UserId");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var resume = await _db.Resumes
            .Include(r => r.WorkExperiences)
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId, ct);

        if (resume == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // 2. Update parent scalar properties cleanly using SetValues
        _db.Entry(resume).CurrentValues.SetValues(req);

        // 3. Sync Work Experiences (Add/Update/Delete)
        UpdateWorkExperiences(resume, req.WorkExperiences);

        resume.ExperienceYears = ExperienceCalculator.CalculateTotalYears(resume.WorkExperiences);
        resume.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        // 4. Meilisearch Background Sync (Don't block the HTTP response waiting for search index tasks)
        var index = _meilisearchClient.Index("resumes");
        await index.AddDocumentsAsync([MeilisearchConverter.ConvertResume(resume)], cancellationToken: ct);

        await Send.NoContentAsync(ct);
    }

    private void UpdateWorkExperiences(Resume resume, List<WorkExperienceDto> incomingExperiences)
    {
        var incomingIds = incomingExperiences
            .Where(w => w.Id.HasValue && w.Id != Guid.Empty)
            .Select(w => w.Id!.Value)
            .ToHashSet();

        // DELETE: Remove experiences not present in the incoming payload
        // 1. Identify which items to remove
        var experiencesToRemove = resume.WorkExperiences
            .Where(we => !incomingIds.Contains(we.Id))
            .ToList();

        // 2. Remove them from the collection
        foreach (var toRemove in experiencesToRemove)
            resume.WorkExperiences.Remove(toRemove);

        foreach (var incoming in incomingExperiences)
        {
            if (incoming.Id.HasValue && incoming.Id != Guid.Empty)
            {
                // UPDATE: EF tracks changes automatically via SetValues
                var existing = resume.WorkExperiences.FirstOrDefault(we => we.Id == incoming.Id);
                if (existing != null)
                {
                    _db.Entry(existing).CurrentValues.SetValues(incoming);
                }
            }
            else
            {
                // ADD: Map to domain object
                resume.WorkExperiences.Add(new WorkExperience
                {
                    CompanyName = incoming.CompanyName,
                    Position = incoming.Position,
                    StartDate = incoming.StartDate,
                    EndDate = incoming.EndDate,
                    Description = incoming.Description,
                    Technologies = incoming.Technologies
                });
            }
        }
    }
}