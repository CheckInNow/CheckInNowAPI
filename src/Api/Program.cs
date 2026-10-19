using Api.Endpoints;
using Api.Extensions;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using ServiceDefaults;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Aspire service defaults (OpenTelemetry, health checks, service discovery)
    builder.AddServiceDefaults();

    // Serilog
    builder.Host.UseSerilog((context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration));

    // Aspire-managed PostgreSQL
    builder.AddNpgsqlDbContext<AppDbContext>("db"); 

    // Aspire-managed Redis (for HybridCache L2)
    builder.AddRedisDistributedCache("cache");

    // Application & Infrastructure
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Global exception handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // OpenAPI with JWT Bearer security scheme
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            var info = document.Info ?? new Microsoft.OpenApi.OpenApiInfo();
            info.Title = "Clean Architecture API v1.0.0";
            document.Info = info;

            var components = document.Components ?? new Microsoft.OpenApi.OpenApiComponents();
            components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
            components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            };

            document.Components = components;

            var schemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer");
            var securityRequirement = new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                [schemeReference] = new List<string>()
            };

            document.Security ??= [];
            document.Security.Add(securityRequirement);
            return Task.CompletedTask;
        });
    });

    // ProblemDetails
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Global exception handler
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTheme(ScalarTheme.Alternate);
            options.WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging();

    // Map endpoints
    app.MapIdentityEndpoints();
    app.MapTodoEndpoints();

    // Aspire default endpoints (health, alive)
    app.MapDefaultEndpoints();

    // Seed database in development
    if (app.Environment.IsDevelopment())
    {
        await AppDbSeeder.SeedAsync(app.Services);
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
