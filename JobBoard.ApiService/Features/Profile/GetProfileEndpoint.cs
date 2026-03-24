using FastEndpoints;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Profile.Models;

namespace JobBoard.ApiService.Features.Profile;

public class GetProfileEndpoint : EndpointWithoutRequest<ProfileResponse>
{
    private readonly JobPortalDbContext _db;

    public GetProfileEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/profile");
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

        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(new ProfileResponse { Id = user.Id, Nickname = user.Nickname }, ct);
    }
}