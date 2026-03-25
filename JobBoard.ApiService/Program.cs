using FastEndpoints;
using FastEndpoints.Security;
using JobBoard.ApiService.Common;
using JobBoard.ApiService.Common.PasswordHashing;
using JobBoard.ApiService.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("jobboarddb");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<JobPortalDbContext>(options =>
    options.UseNpgsql(dataSource));

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

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

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
