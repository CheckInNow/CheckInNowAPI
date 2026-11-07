
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Email;
using Application.Abstractions.Identity;
using Application.Abstractions.OAuth;
using Application.Abstractions.Shared;
using Domain.Entities;
using Infrastructure.Email;
using Infrastructure.Identity;
using Infrastructure.OAuth;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence();
        services.AddAuth(configuration);
        services.AddCachingServices();
        services.AddEmail(configuration);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
    }

    private static void AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ??
                        throw new InvalidOperationException("Jwt:Secret is not configured"))),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IGoogleTokenVerifier, IGoogleTokenVerifier>();
    }

    private static void AddCachingServices(this IServiceCollection services)
    {
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });
    }

    private static void AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFrontendLinkBuilder, FrontendLinkBuilder>();

        if (string.Equals(configuration["EMAIL_PROVIDER"], "mailgun", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IEmailSender, MailgunEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, MailpitEmailSender>();
        }
    }
}
