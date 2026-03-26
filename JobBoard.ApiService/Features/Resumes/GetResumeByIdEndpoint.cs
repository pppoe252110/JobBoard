using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Resumes.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Resumes;

public class GetResumeByIdEndpoint : Endpoint<GetResumeByIdRequest, ResumeResponse>
{
    private readonly JobPortalDbContext _db;

    public GetResumeByIdEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/resumes/{id}");
        Roles("User");
    }

    public override async Task HandleAsync(GetResumeByIdRequest req, CancellationToken ct)
    {
        var resume = await _db.Resumes
            .Where(r => r.Id == req.Id && r.IsVisible)
            .Select(r => new ResumeResponse
            {
                Id = r.Id,
                FullName = r.FullName,
                Title = r.Title,
                Location = r.Location,
                ExpectedSalary = r.ExpectedSalary,
                Skills = r.Skills,
                IsVisible = r.IsVisible,
                AboutMe = r.AboutMe,
                ContactMethods = r.ContactMethods,
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
            .FirstOrDefaultAsync(ct);

        if (resume == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(resume, ct);
    }
}

public class GetResumeByIdRequest
{
    public Guid Id { get; set; }
}