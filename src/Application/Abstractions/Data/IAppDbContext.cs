
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<TodoItem> Todos { get; }
    DbSet<Conference> Conferences { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
