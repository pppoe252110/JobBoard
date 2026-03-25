using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Resumes.Models;

namespace JobBoard.ApiService.Features.Resumes;

public class CreateResumeEndpoint : Endpoint<CreateResumeRequest>
{
    private readonly JobPortalDbContext _db;

    public CreateResumeEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Post("/resumes");
        Roles("User");
    }

    public override async Task HandleAsync(CreateResumeRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = req.FullName,
            Title = req.Title,
            Location = req.Location,
            Skills = req.Skills,
            IsVisible = req.IsVisible,
            AboutMe = req.AboutMe,
            ContactMethods = req.ContactMethods,
            WorkExperiences = req.WorkExperiences.Select(we => new WorkExperience
            {
                Id = we.Id ?? Guid.NewGuid(),
                CompanyName = we.CompanyName,
                Position = we.Position,
                StartDate = we.StartDate,
                EndDate = we.EndDate,
                Description = we.Description,
                Technologies = we.Technologies
            }).ToList()
        };

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync(ct);

        await Send.OkAsync(new { resume.Id }, ct);
    }
}