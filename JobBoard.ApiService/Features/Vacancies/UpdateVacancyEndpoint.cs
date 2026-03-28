using FastEndpoints;
using Meilisearch;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Vacancies.Models;
using JobBoard.ApiService.Utils;

namespace JobBoard.ApiService.Features.Vacancies;

public class UpdateVacancyEndpoint : Endpoint<UpdateVacancyRequest>
{
    private readonly JobPortalDbContext _db;
    private readonly MeilisearchClient _meilisearchClient;

    public UpdateVacancyEndpoint(JobPortalDbContext db, MeilisearchClient meilisearchClient)
    {
        _db = db;
        _meilisearchClient = meilisearchClient;
    }

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
        vacancy.Location = req.Location ?? string.Empty;
        vacancy.IsRemote = req.IsRemote;

        await _db.SaveChangesAsync(ct);

        var index = _meilisearchClient.Index("vacancies");
        var taskInfo = await index.AddDocumentsAsync([MeilisearchConverter.ConvertVacancy(vacancy)], "id", ct);
        var finishedTask = await index.WaitForTaskAsync(taskInfo.TaskUid, cancellationToken: ct);

        if (finishedTask.Status == TaskInfoStatus.Failed)
        {
            Console.WriteLine($"Error: {finishedTask.Error}");
            foreach (var item in finishedTask.Error)
            {
                Console.WriteLine($"Error: {item.Key + " " + item.Value}");
            }
        }
        await Send.NoContentAsync(ct);
    }
}