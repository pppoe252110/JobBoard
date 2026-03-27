using FastEndpoints;
using JobBoard.ApiService.Common.PasswordHashing;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.ApiService.Features.Auth;

public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest>
{
    private readonly JobPortalDbContext _db;
    private readonly IPasswordHasher _hasher;

    public ResetPasswordEndpoint(JobPortalDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public override void Configure()
    {
        Post("/auth/reset-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(req.Token);
        var dbTokenBytes = System.Text.Encoding.UTF8.GetBytes(user.PasswordResetToken ?? "");

        if (user == null || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(tokenBytes, dbTokenBytes) || user.ResetTokenExpiresAt < DateTime.UtcNow)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Update password
        user.PasswordHash = _hasher.Hash(req.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpiresAt = null;

        await _db.SaveChangesAsync(ct);

        await Send.OkAsync(new { Message = "Password has been reset successfully." }, ct);
    }
}