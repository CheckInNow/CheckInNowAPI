using Application.Abstractions.Shared;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Shared;

public sealed class FrontendLinkBuilder(IConfiguration configuration) : IFrontendLinkBuilder
{
    public string BuildFrontendLink(string action, Guid userId, string token)
    {
        var frontendUrl = configuration["Frontend:Url"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Frontend:Url is not configured");

        // /confirm-email, /reset-password etc
        return $"{frontendUrl}/{action}?userId={userId}&token={token}";
    }
}
