using Application.Abstractions.OAuth;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.OAuth;

public sealed class GoogleTokenVerifier(IConfiguration config) : IGoogleTokenVerifier
{
    public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [config["Authentication:Google:ClientId"]
                ?? throw new InvalidOperationException("Google ClientId is not configured")]
        };

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return new GoogleUserInfo(payload.Subject, payload.Email, payload.EmailVerified, payload.GivenName, payload.FamilyName);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
