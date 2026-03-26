using FastEndpoints;
using JobBoard.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Applications;

public class UpdateApplicationStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateApplicationStatusEndpoint : Endpoint<UpdateApplicationStatusRequest>
{
    private readonly JobPortalDbContext _db;

    public UpdateApplicationStatusEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Patch("/applications/{id}/status");
        Roles("User");
    }

    public override async Task HandleAsync(UpdateApplicationStatusRequest req, CancellationToken ct)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var applicationId = Route<Guid>("id");
        var application = await _db.Applications.FindAsync(new object[] { applicationId }, ct);

        if (application == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Verify the user modifying the status owns the vacancy
        var vacancy = await _db.Vacancies.FindAsync(new object[] { application.VacancyId }, ct);
        if (vacancy == null || vacancy.UserId != userId)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Validate the status input
        var validStatuses = new[] { "Applied", "Review", "Interview", "Offered", "Rejected" };
        if (!validStatuses.Contains(req.Status))
        {
            AddError("Invalid status provided.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        application.Status = req.Status;
        await _db.SaveChangesAsync(ct);

        await Send.OkAsync(new { Message = "Application status updated successfully." }, ct);
    }
}