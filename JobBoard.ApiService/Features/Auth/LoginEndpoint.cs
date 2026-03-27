using FastEndpoints;
using JobBoard.ApiService.Common.PasswordHashing;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Auth;

public class LoginEndpoint : Endpoint<LoginRequest, object>
{
    private readonly JobPortalDbContext _db;
    private readonly IPasswordHasher _hasher;

    public LoginEndpoint(JobPortalDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("AuthPolicy"));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        // If user not found OR password hash is missing → invalid
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !_hasher.Verify(req.Password, user.PasswordHash))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Return user data
        await Send.OkAsync(new
        {
            userId = user.Id,
            nickname = user.Nickname,
            email = user.Email
        }, ct);
    }
}