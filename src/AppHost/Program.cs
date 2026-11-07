var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(c => c
        .WithContainerName("aspire-pgadmin")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithContainerName("aspire-postgres")
    .WithImageTag("18")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("db");

var redis = builder.AddRedis("cache")
    .WithContainerName("aspire-redis")
    .WithRedisInsight(c => c
        .WithContainerName("aspire-redis-insight")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var mailpit = builder.AddMailPit("mailpit")
    .WithImageTag("v1.33.0")
    .WithContainerName("aspire-mailpit")
    .WithLifetime(ContainerLifetime.Persistent);

var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var googleClientId = builder.AddParameter("google-client-id", secret: true);

builder.AddProject<Projects.Api>("api")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Authentication__Google__ClientId", googleClientId)
    .WithEnvironment("EMAIL_PROVIDER", "mailpit")
    .WithEnvironment("MAILPIT_HOST", mailpit.Resource.Host)
    .WithEnvironment("MAILPIT_SMTP_PORT", mailpit.Resource.Port)
    .WithEnvironment("Frontend__Url", "http://localhost:5173")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(redis)
    .WaitFor(redis)
    .WaitFor(mailpit)
    .WithExternalHttpEndpoints();

builder.Build().Run();
