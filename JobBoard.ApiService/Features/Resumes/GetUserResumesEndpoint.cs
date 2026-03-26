using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Resumes.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Resumes;

public class GetUserResumesEndpoint : EndpointWithoutRequest<List<ResumeResponse>>
{
    private readonly JobPortalDbContext _db;

    public GetUserResumesEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/profile/resumes");
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

        var resumes = await _db.Resumes
            .Where(r => r.UserId == userId)
                .Select(r => new ResumeResponse
                {
                    Id = r.Id,
                    FullName = r.FullName,
                    Title = r.Title,
                    Location = r.Location,
                    ContactMethods = r.ContactMethods,
                    Skills = r.Skills,
                    IsVisible = r.IsVisible,
                    AboutMe = r.AboutMe,
                    ExpectedSalary = r.ExpectedSalary,
                    WorkExperiences = r.WorkExperiences.Select(we => new WorkExperienceDto
                    {
                        Id = we.Id,
                        CompanyName = we.CompanyName,
                        Position = we.Position,
                        StartDate = we.StartDate,
                        EndDate = we.EndDate,
                        Description = we.Description,
                        Technologies = we.Technologies
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync(ct);

        await Send.OkAsync(resumes, ct);
    }
}