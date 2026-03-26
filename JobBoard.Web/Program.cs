using JobBoard.Web.Components;
using JobBoard.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();

// DI
builder.Services.AddScoped<ThemeService>();
builder.Services.AddTransient<CookieDelegatingHandler>();

// DataProtection (shared between API + Web)
var sharedKeyPath = Path.Combine(Path.GetTempPath(), "jobboard-auth-keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(sharedKeyPath))
    .SetApplicationName("JobBoard");

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
})
.AddHttpMessageHandler<CookieDelegatingHandler>();

builder.Services.AddHttpClient<VacancyApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
}).AddHttpMessageHandler<CookieDelegatingHandler>();

builder.Services.AddHttpClient<ResumeApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://apiservice");
}).AddHttpMessageHandler<CookieDelegatingHandler>();
// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "JobBoardAuth";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/Forbidden/";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapPost("/auth/login-callback", async (
    HttpContext context,
    [FromForm] string email,
    [FromForm] string password,
    [FromServices] AuthApiClient authApi) =>
{
    var loginResult = await authApi.LoginAsync(email, password);

    if (loginResult != null)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", loginResult.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, loginResult.UserId.ToString()),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Email, loginResult.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Results.Redirect("/");
    }

    return Results.Redirect("/login?error=true");
});

app.MapPost("/auth/logout-callback", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();
