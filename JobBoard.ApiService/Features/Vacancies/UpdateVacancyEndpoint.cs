using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Vacancies.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Vacancies;

public class UpdateVacancyEndpoint : Endpoint<UpdateVacancyRequest>
{
    private readonly JobPortalDbContext _db;

    public UpdateVacancyEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/vacancies/{id}");
        Roles("User");
    }

    public override async Task HandleAsync(UpdateVacancyRequest req, CancellationToken ct)
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

        var vacancy = await _db.Vacancies.FindAsync(new object[] { req.Id }, ct);
        if (vacancy == null || vacancy.UserId != userId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        vacancy.Title = req.Title;
        vacancy.DescriptionMarkdown = req.DescriptionMarkdown;
        vacancy.SalaryFrom = req.SalaryFrom;
        vacancy.SalaryTo = req.SalaryTo;

        await _db.SaveChangesAsync(ct);
        await Send.OkAsync(ct);
    }
}