namespace JobBoard.ApiService.Features.Auth.Models
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
