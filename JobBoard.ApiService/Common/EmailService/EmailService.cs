namespace JobBoard.ApiService.Common;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendAuthCode(string email, string code)
    {
        _logger.LogInformation("Auth code for {Email}: {Code}", email, code);
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetEmail(string email, string resetToken)
    {
        // In production, send a real email with a link: /reset-password?token={resetToken}&email={email}
        _logger.LogInformation("Password reset link for {Email}: /reset-password?token={Token}&email={Email}", email, resetToken, email);
        await Task.CompletedTask;
    }
}