using Domain.Common;

namespace Domain.Entities;

public sealed class SessionCheckIn : BaseEntity
{
    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;
    public Guid SessionId { get; set; }
    public Session Session { get; set; } = default!;
    public DateTimeOffset CheckedInAt { get; set; }
}
