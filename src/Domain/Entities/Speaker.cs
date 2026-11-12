using Domain.Common;

namespace Domain.Entities;

public sealed class Speaker : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public Guid ConferenceId { get; set; }
    public Conference Conference { get; set; } = default!;
}
