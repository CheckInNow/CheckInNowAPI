var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(c => c
        .WithContainerName("aspire-pgadmin")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithContainerName("aspire-postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("db");

var redis = builder.AddRedis("cache")
    .WithContainerName("aspire-redis")
    .WithRedisInsight(c => c
        .WithContainerName("aspire-redis-insight")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

builder.AddProject<Projects.Api>("api")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithReference(database)
    .WaitFor(database)
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

builder.Build().Run();
