using Domain.Common;

namespace Domain.Entities;

public sealed class Conference : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int ParticipantLimit { get; set; }
    public Guid OrganizerId { get; set; }
    public ApplicationUser Organizer { get; set; } = default!;
    public DateTimeOffset? DeletedAt { get; set; } = null;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Speaker> Speakers { get; set; } = new List<Speaker>();
}
