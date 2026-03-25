using FastEndpoints;
using JobBoard.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Resumes;

public class DeleteResumeEndpoint : EndpointWithoutRequest
{
    private readonly JobPortalDbContext _db;

    public DeleteResumeEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/resumes/{id}");
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
        var resume = await _db.Resumes.FindAsync(new object[] { id }, ct);
        if (resume == null || resume.UserId != userId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        _db.Resumes.Remove(resume);
        await _db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}