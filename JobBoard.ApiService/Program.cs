using FastEndpoints;
using FastEndpoints.Security;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Common.PasswordHashing;
using JobBoard.ApiService.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<JobPortalDbContext>("jobboarddb");

//Cookie Setup and Data Protection
var sharedKeyPath = Path.Combine(Path.GetTempPath(), "jobboard-auth-keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(sharedKeyPath))
    .SetApplicationName("JobBoard");

builder.Services.AddAuthenticationCookie(validFor: TimeSpan.FromMinutes(30), options =>
{
    options.Cookie.Name = "JobBoardAuth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddAuthorization()
.AddFastEndpoints();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

//DI
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints();
if (app.Environment.IsDevelopment())
{
    var application = app.Services.CreateScope().ServiceProvider.GetRequiredService<JobPortalDbContext>();

    var pendingMigrations = await application.Database.GetPendingMigrationsAsync();
    if (pendingMigrations != null)
        await application.Database.MigrateAsync();
}
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}