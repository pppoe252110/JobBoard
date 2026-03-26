using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Vacancies.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Vacancies;

public class GetVacancyListEndpoint : EndpointWithoutRequest<List<VacancyResponse>>
{
    private readonly JobPortalDbContext _db;

    public GetVacancyListEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/vacancies");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var vacancies = await _db.Vacancies
            .Where(v => !v.IsArchived)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VacancyResponse
            {
                Id = v.Id,
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