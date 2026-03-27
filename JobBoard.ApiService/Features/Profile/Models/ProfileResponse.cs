namespace JobBoard.ApiService.Features.Profile.Models;

public record ProfileResponse
{
    public Guid Id { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}