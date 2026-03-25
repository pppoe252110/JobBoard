using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Resumes.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Resumes;

public class UpdateResumeEndpoint : Endpoint<UpdateResumeRequest>
{
    private readonly JobPortalDbContext _db;

    public UpdateResumeEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/resumes/{id}");
        Roles("User");
    }

    public override async Task HandleAsync(UpdateResumeRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var id = Route<Guid>("id");
        if (req.Id != id)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var resume = await _db.Resumes
            .Include(r => r.WorkExperiences)
            .FirstOrDefaultAsync(r => r.Id == req.Id, ct);

        if (resume == null || resume.UserId != userId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Update scalar properties
        resume.FullName = req.FullName;
        resume.Title = req.Title;
        resume.Location = req.Location;
        resume.Skills = req.Skills;
        resume.IsVisible = req.IsVisible;
        resume.AboutMe = req.AboutMe;

        // Update work experiences
        var incomingIds = req.WorkExperiences.Where(we => we.Id.HasValue).Select(we => we.Id.Value).ToList();
        var toRemove = resume.WorkExperiences.Where(we => !incomingIds.Contains(we.Id)).ToList();
        foreach (var we in toRemove)
            _db.WorkExperiences.Remove(we);

        foreach (var weDto in req.WorkExperiences)
        {
            if (weDto.Id.HasValue)
            {
                var existing = resume.WorkExperiences.FirstOrDefault(we => we.Id == weDto.Id);
                if (existing != null)
                {
                    existing.CompanyName = weDto.CompanyName;
                    existing.Position = weDto.Position;
                    existing.StartDate = weDto.StartDate;
                    existing.EndDate = weDto.EndDate;
                    existing.Description = weDto.Description;
                    existing.Technologies = weDto.Technologies;
                }
            }
            else
            {
                resume.WorkExperiences.Add(new WorkExperience
                {
                    Id = Guid.NewGuid(),
                    CompanyName = weDto.CompanyName,
                    Position = weDto.Position,
                    StartDate = weDto.StartDate,
                    EndDate = weDto.EndDate,
                    Description = weDto.Description,
                    Technologies = weDto.Technologies
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        await Send.OkAsync(ct);
    }
}