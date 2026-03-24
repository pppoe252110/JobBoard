using FastEndpoints;
using FastEndpoints.Security;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Profile;

public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequest>
{
    private readonly JobPortalDbContext _db;

    public UpdateProfileEndpoint(JobPortalDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/profile");
        Roles("User");
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
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

        // Check nickname uniqueness (excluding current user)
        if (await _db.Users.AnyAsync(u => u.Nickname == req.Nickname && u.Id != userId, ct))
        {
            AddError(r => r.Nickname, "Nickname already taken");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        user.Nickname = req.Nickname;
        await _db.SaveChangesAsync(ct);

        // Update the nickname claim in the current cookie
        await CookieAuth.SignInAsync(u =>
        {
            u.Roles.Add("User");
            u.Claims.Add(("UserId", user.Id.ToString()));
            u.Claims.Add(("Nickname", user.Nickname));
        });

        await Send.OkAsync(new { Message = "Profile updated" }, ct);
    }
}