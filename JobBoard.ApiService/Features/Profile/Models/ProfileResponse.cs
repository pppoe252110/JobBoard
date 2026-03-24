namespace JobBoard.ApiService.Features.Profile.Models;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
}