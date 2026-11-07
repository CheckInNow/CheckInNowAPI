namespace Application.Abstractions.OAuth;

public interface IGoogleTokenVerifier
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
