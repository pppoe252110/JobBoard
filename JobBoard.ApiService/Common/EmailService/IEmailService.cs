namespace JobBoard.ApiService.Common;

public interface IEmailService
{
    Task SendAuthCode(string email, string code);
    Task SendPasswordResetEmail(string email, string resetToken);
}