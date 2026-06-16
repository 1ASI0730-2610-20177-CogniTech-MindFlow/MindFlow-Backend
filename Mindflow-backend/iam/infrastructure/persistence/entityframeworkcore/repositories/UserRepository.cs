using Microsoft.EntityFrameworkCore;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;


namespace Mindflow_backend.iam.infrastructure.persistence.entityframeworkcore.repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email)
        => await Context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> FindByGoogleIdAsync(string googleId)
        => await Context.Set<User>().FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public bool ExistsByEmail(string email)
        => Context.Set<User>().Any(u => u.Email == email);
}