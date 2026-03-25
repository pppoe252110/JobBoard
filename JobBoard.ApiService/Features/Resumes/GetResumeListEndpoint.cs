using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Resumes.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Resumes;

public class GetResumeListEndpoint : EndpointWithoutRequest<List<ResumeResponse>>
{
    private readonly JobPortalDbContext _db;

    public GetResumeListEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/resumes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var resumes = await _db.Resumes
            .Where(r => r.IsVisible)
            .Select(r => new ResumeResponse
            {
                Id = r.Id,
                FullName = r.FullName,
                Title = r.Title,
                Location = r.Location,
                Skills = r.Skills,
                IsVisible = r.IsVisible,
                AboutMe = r.AboutMe,
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