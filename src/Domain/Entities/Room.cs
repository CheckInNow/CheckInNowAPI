using Domain.Common;

namespace Domain.Entities;

public sealed class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public Guid ConferenceId { get; set; }
    public Conference Conference { get; set; } = default!;
}
