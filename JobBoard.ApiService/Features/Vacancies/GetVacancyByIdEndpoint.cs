using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Vacancies.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Vacancies;

public class GetVacancyByIdEndpoint : Endpoint<GetVacancyByIdRequest, VacancyResponse>
{
    private readonly JobPortalDbContext _db;

    public GetVacancyByIdEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/vacancies/{id}");
        Roles("User");
    }

    public override async Task HandleAsync(GetVacancyByIdRequest req, CancellationToken ct)
    {
        var vacancy = await _db.Vacancies
            .Where(v => v.Id == req.Id && !v.IsArchived)
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
            .FirstOrDefaultAsync(ct);

        if (vacancy == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(vacancy, ct);
    }
}

public class GetVacancyByIdRequest
{
    public Guid Id { get; set; }
}