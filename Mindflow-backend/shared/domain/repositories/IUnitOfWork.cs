namespace Mindflow_backend.Shared.Domain.Repositories;

/// <summary>
///     Unit of work interface for all repositories
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Save changes to the repository
    /// </summary>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
