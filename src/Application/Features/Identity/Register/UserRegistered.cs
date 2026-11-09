using MediatR;

namespace Application.Features.Identity.Register;

public sealed record UserRegistered(Guid UserId) : INotification;
