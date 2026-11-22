using Domain.Common;

namespace Domain.Entities;

public sealed class ConferenceCheckIn : BaseEntity
{
    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;
    public DateTimeOffset CheckedInAt { get; set; }
}
