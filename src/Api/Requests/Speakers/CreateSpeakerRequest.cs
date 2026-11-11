namespace Api.Requests.Speakers;

public sealed record CreateSpeakerRequest(string FirstName, string LastName, string? Bio);
