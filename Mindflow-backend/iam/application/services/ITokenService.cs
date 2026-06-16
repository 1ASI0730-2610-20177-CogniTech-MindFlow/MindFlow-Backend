using Mindflow_backend.iam.domain.model.aggregates;

namespace Mindflow_backend.iam.application.services;

public interface ITokenService
{
    string GenerateToken(User user);
}
