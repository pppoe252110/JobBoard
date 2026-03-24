var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

// Chain .WithPortBinding to lock the host port to 5432
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
                      .WithDataVolume("pgdata")
                      .WithHostPort(7777);

var db = postgres.AddDatabase("jobboarddb");

var apiService = builder.AddProject<Projects.JobBoard_ApiService>("apiservice")
    .WithReference(db)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.JobBoard_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();