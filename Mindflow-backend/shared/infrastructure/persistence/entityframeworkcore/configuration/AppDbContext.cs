// Agrega aquí los usings de los extension methods de tus bounded contexts, por ejemplo:
// using Mindflow_backend.[BoundedContext].Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Application database context
/// </summary>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Aplica el interceptor de auditoría automático para todas las entidades que implementen IAuditableEntity
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Aquí agrega la configuración de cada bounded context de tu proyecto, por ejemplo:
        // builder.ApplyMiBoundedContextConfiguration();

        // Aplica la convención de nombres snake_case + pluralización para toda la base de datos
        builder.UseSnakeCaseNamingConvention();
    }
}
