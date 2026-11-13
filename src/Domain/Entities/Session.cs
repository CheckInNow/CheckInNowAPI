using Domain.Common;

namespace Domain.Entities;

public sealed class Session : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public DateTimeOffset? DeletedAt { get; set; } = null;
    public Guid ConferenceId { get; set; }
    public Conference Conference { get; set; } = default!;
    public Speaker? Speaker { get; set; } = null;
    public Guid? SpeakerId { get; set; } = null;
    public Room? Room { get; set; } = null;
    public Guid? RoomId { get; set; } = null;
}
