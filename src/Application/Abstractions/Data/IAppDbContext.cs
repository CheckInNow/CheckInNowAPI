
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<Conference> Conferences { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Speaker> Speakers { get; }
    DbSet<Session> Sessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
