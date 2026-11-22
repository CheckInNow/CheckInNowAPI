using Domain.Common;

namespace Domain.Entities;

public sealed class Registration : AuditableEntity
{
    public Guid ConferenceId { get; set; }
    public Conference Conference { get; set; } = default!;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public Guid TicketCode { get; set; } = Guid.NewGuid();
    public ConferenceCheckIn? ConferenceCheckIn { get; set; }
}
