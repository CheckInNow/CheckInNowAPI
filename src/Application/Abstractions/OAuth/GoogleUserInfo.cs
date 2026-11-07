namespace Application.Abstractions.OAuth;

public sealed record GoogleUserInfo(string Subject, string Email, bool EmailVerified, string GivenName, string? FamilyName);
