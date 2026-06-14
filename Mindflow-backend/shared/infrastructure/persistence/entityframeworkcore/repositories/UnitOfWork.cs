using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Unit of work for the application.
/// </summary>
/// <remarks>
///     This class is used to save changes to the database context.
///     It implements the IUnitOfWork interface.
/// </remarks>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
