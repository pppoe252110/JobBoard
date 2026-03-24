using FastEndpoints;
using FastEndpoints.Security;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Common.PasswordHashing;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Auth.Models;
using JobBoard.ApiService.Features.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Auth;

public class RegisterEndpoint : Endpoint<RegisterRequest>
{
    private readonly JobPortalDbContext _db;
    private readonly IPasswordHasher _hasher;

    public RegisterEndpoint(JobPortalDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        // Check if email already used
        if (await _db.Users.AnyAsync(u => u.Email == req.Email, ct))
        {
            AddError(r => r.Email, "Email already registered");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        // Check nickname uniqueness (optional but recommended)
        if (await _db.Users.AnyAsync(u => u.Nickname == req.Nickname, ct))
        {
            AddError(r => r.Nickname, "Nickname already taken");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            Nickname = req.Nickname,
            PasswordHash = _hasher.Hash(req.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        // Automatically log in after registration
        await CookieAuth.SignInAsync(u =>
        {
            u.Roles.Add("User");
            u.Claims.Add(("UserId", user.Id.ToString()));
            u.Claims.Add(("Nickname", user.Nickname));   // store nickname in claims (no email)
        });

        await Send.OkAsync(new { Message = "Registration successful" }, ct);
    }
}