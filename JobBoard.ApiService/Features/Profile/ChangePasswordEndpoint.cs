using FastEndpoints;
using JobBoard.ApiService.Common.PasswordHashing;
using JobBoard.ApiService.Data;
using JobBoard.ApiService.Features.Profile.Models;

namespace JobBoard.ApiService.Features.Profile
{
    public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest>
    {
        private readonly JobPortalDbContext _db;
        private readonly IPasswordHasher _hasher;

        public ChangePasswordEndpoint(JobPortalDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public override void Configure()
        {
            Put("/profile/password");
            Roles("User");
        }

        public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
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

            if (!_hasher.Verify(req.CurrentPassword, user.PasswordHash))
            {
                AddError(r => r.CurrentPassword, "Current password is incorrect");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            user.PasswordHash = _hasher.Hash(req.NewPassword);
            await _db.SaveChangesAsync(ct);

            await Send.OkAsync(new { Message = "Password changed" }, ct);
        }
    }
}
