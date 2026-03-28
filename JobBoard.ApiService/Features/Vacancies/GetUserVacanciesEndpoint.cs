using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Vacancies.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Vacancies;

public class GetUserVacanciesEndpoint : EndpointWithoutRequest<List<VacancyResponse>>
{
    private readonly JobPortalDbContext _db;

    public GetUserVacanciesEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/profile/vacancies");
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

        var vacancies = await _db.Vacancies
            .Where(v => v.UserId == userId)
            .Select(v => new VacancyResponse
            {
                Id = v.Id,
                UserId = v.UserId,
                Title = v.Title,
                DescriptionMarkdown = v.DescriptionMarkdown,
                SalaryFrom = v.SalaryFrom,
                SalaryTo = v.SalaryTo,
                Location = v.Location,
                IsRemote = v.IsRemote,
                CreatedAt = v.CreatedAt,
                IsArchived = v.IsArchived
            })
            .ToListAsync(ct);

        await Send.OkAsync(vacancies, ct);
    }
}