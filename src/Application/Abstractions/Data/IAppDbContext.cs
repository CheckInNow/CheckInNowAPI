using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<IdentityRole<Guid>> Roles { get; }
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    DbSet<Conference> Conferences { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Speaker> Speakers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
