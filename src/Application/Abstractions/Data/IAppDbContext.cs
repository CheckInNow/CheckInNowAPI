
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<Conference> Conferences { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
