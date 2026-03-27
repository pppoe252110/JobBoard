using Microsoft.AspNetCore.Http.HttpResults;

namespace JobBoard.Web.Services;

public class AuthApiClient(HttpClient httpClient)
{
    public async Task<HttpResponseMessage> RegisterAsync(string email, string nickname, string password)
    {
        var payload = new { email, nickname, password };
        return await httpClient.PostAsJsonAsync("/auth/register", payload);
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var response = await httpClient.PostAsJsonAsync("/auth/login", new
        {
            email,
            password
        });

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<ProfileResponse?> GetProfileAsync()
    {
        return await httpClient.GetFromJsonAsync<ProfileResponse>("/profile");
    }

    public async Task<bool> UpdateProfileAsync(string nickname)
    {
        var response = await httpClient.PutAsJsonAsync("/profile", new { nickname });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var response = await httpClient.PutAsJsonAsync("/profile/password", new { currentPassword, newPassword });
        return response.IsSuccessStatusCode;
    }

    public async Task<HttpResponseMessage> ForgotPasswordAsync(string email)
    {
        return await httpClient.PostAsJsonAsync("/auth/forgot-password", new { email });
    }

    public async Task<HttpResponseMessage> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var payload = new { email, token, newPassword };
        return await httpClient.PostAsJsonAsync("/auth/reset-password", payload);
    }
}

public record LoginResponse(Guid UserId, string Nickname, string Email);
public record ProfileResponse(Guid Id, string Nickname, string Email, DateTime CreatedAt);