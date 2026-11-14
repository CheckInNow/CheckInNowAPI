
using Domain.Entities;

namespace Application.Abstractions.Identity;

public interface ITokenService
{
    Task<TokenResponse> GenerateTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
    string GetUserIdFromExpiredToken(string accessToken);
}

public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
