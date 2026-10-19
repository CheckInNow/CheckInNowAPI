
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;
public interface IAppDbContext
{
    DbSet<TodoItem> Todos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
