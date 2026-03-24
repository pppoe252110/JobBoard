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
        var request = new HttpRequestMessage(HttpMethod.Get, "/profile");

        // This tells the browser to include cookies
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ProfileResponse>();
        }
        return null;
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
public record ProfileResponse(Guid Id, string Nickname);