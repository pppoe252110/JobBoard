using FastEndpoints;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace JobBoard.ApiService.Features.Auth;

public class ForgotPasswordEndpoint : Endpoint<ForgotPasswordRequest>
{
    private readonly JobPortalDbContext _db;
    private readonly IEmailService _emailService;

    public ForgotPasswordEndpoint(JobPortalDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public override void Configure()
    {
        Post("/auth/forgot-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ForgotPasswordRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);
        if (user == null)
        {
            // For security, do not reveal that the email doesn't exist
            await Send.OkAsync(new { Message = "If the email is registered, a reset link has been sent." }, ct);
            return;
        }

        // Generate a secure random token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        user.PasswordResetToken = token;
        user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1); // 1 hour validity

        await _db.SaveChangesAsync(ct);

        await _emailService.SendPasswordResetEmail(req.Email, token);

        await Send.OkAsync(new { Message = "If the email is registered, a reset link has been sent." }, ct);
    }
}