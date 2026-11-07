namespace Application.Abstractions.Shared;

public interface IFrontendLinkBuilder
{
    string BuildFrontendLink(string action, Guid userId, string token);
}
