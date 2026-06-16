using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.Shared.Domain.Repositories;


namespace Mindflow_backend.iam.domain.repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(string email);
    bool ExistsByEmail(string email);
}